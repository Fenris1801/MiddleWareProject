import { Client } from '@stomp/stompjs';

const template = document.createElement("template");
template.innerHTML = `
<style>
  :host {
    display: block;
    font-size: 0.85rem;
  }
  h2 { margin: 0 0 0.5rem; font-size: 1rem; }
  .toolbar { display: flex; gap: 0.5rem; margin-bottom: 0.5rem; }
  button {
    padding: 0.3rem 0.5rem;
    border-radius: 4px; border: 1px solid #ccc;
    background: #f4f4f4; cursor: pointer; font-size: 0.8rem;
  }
  button:hover { background: #e2e2e2; }
  .messages, .steps {
    max-height: 200px; overflow-y: auto;
    border: 1px solid #eee; border-radius: 4px;
    padding: 0.4rem; background: #fafafa;
  }
  .message { margin-bottom: 0.25rem; }
  .message .time { color: #666; margin-right: 0.25rem; }
  .topics { margin: 0.5rem 0; }
  .steps ol {
    margin: 0;
    padding-left: 1.2rem;
  }
</style>

<h2>Messages & alertes</h2>

<div class="toolbar">
  <button id="connect">Connect</button>
  <button id="disconnect" disabled>Disconnect</button>
  <button id="recompute">Recompute itinerary</button>
</div>

<div class="topics">
  <strong>Topics:</strong>
  <select id="topicSelect" multiple size="6"></select>
  <button id="applySubs" disabled>Apply subscriptions</button>
</div>

<div class="messages"></div>

<div class="infos">
  <strong>Informations de l'itinéraire :</strong>
  <ul></ul>
</div>
<div class="steps">
  <strong>Etapes de l'itinéraire :</strong>
  <ol></ol>
</div>
`;

export class Messages extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: "open" });
    this.shadowRoot.appendChild(template.content.cloneNode(true));

    this.client = null;
    this.subscriptions = {};
  }

  connectedCallback() {
    this.shadowRoot.querySelector("#connect")
      .addEventListener("click", () => this.connect());

    this.shadowRoot.querySelector("#disconnect")
      .addEventListener("click", () => this.disconnect());

    this.shadowRoot.querySelector("#applySubs")
      .addEventListener("click", () => this.updateSubscriptions());

    this.shadowRoot.querySelector("#recompute")
      .addEventListener("click", () => {
        this.addMessage("Recompute requested");
        this.dispatchEvent(new CustomEvent("recompute-itinerary", {
          bubbles: true,
          composed: true
        }));
      });

    this.brokerUrl = this.getAttribute("broker-url") || "ws://localhost:61614";
  }

  addMessage(text) {
    const box = this.shadowRoot.querySelector(".messages");
    const div = document.createElement("div");
    div.className = "message";

    const t = new Date().toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" });

    div.innerHTML = `<span class="time">[${t}]</span>${text}`;
    box.prepend(div);
  }

  connect() {
    this.client = new Client({
      brokerURL: this.brokerUrl,
      reconnectDelay: 5000,
      debug: () => { }
    });

    this.client.onConnect = () => {
      this.shadowRoot.querySelector("#connect").disabled = true;
      this.shadowRoot.querySelector("#disconnect").disabled = false;
      this.shadowRoot.querySelector("#applySubs").disabled = false;

      this.addMessage("Connected to broker");
      this.loadTopics();
    };

    this.client.onStompError = frame => {
      this.addMessage("Broker error: " + frame.headers["message"]);
    };

    this.client.activate();
  }

  disconnect() {
    Object.values(this.subscriptions).forEach(s => s.unsubscribe());
    this.subscriptions = {};

    if (this.client) this.client.deactivate();

    this.shadowRoot.querySelector("#connect").disabled = false;
    this.shadowRoot.querySelector("#disconnect").disabled = true;
    this.shadowRoot.querySelector("#applySubs").disabled = true;
  }

  loadTopics() {
    const select = this.shadowRoot.querySelector("#topicSelect");
    select.innerHTML = "";

    const topics = [
      "weather",
      "pollution",
      "bikeAvailability",
      "traffic",
      "maintenance",
      "safety.alerts"
    ];

    topics.forEach(t => {
      const opt = document.createElement("option");
      opt.value = `/topic/${t}`;
      opt.textContent = t;
      select.appendChild(opt);
    });
  }

  updateSubscriptions() {
    Object.values(this.subscriptions).forEach(s => s.unsubscribe());
    this.subscriptions = {};

    const selected = Array.from(
      this.shadowRoot.querySelector("#topicSelect").selectedOptions
    ).map(o => o.value);

    selected.forEach(dest => {
      const sub = this.client.subscribe(dest, msg => {
        this.addMessage(`[${dest}] ${msg.body}`);
      });
      this.subscriptions[dest] = sub;
    });

    this.addMessage("Subscriptions updated");
  }

  setInfos(infos) {
    const list = this.shadowRoot.querySelector('.infos ul');
    list.innerHTML = '';
    (infos || []).forEach(step => {
      const li = document.createElement('li');
      li.textContent = step;
      list.appendChild(li);
    });
  }

  setSteps(steps) {
    const list = this.shadowRoot.querySelector('.steps ol');
    list.innerHTML = '';
    (steps || []).forEach(step => {
      const li = document.createElement('li');
      li.textContent = step;
      list.appendChild(li);
    });
  }
}

customElements.define("compo-messages", Messages);
