const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      display: block;
      font-size: 0.85rem;
    }
    h2 {
      margin: 0 0 0.5rem;
      font-size: 1rem;
    }
    .toolbar {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 0.5rem;
    }
    button {
      padding: 0.3rem 0.5rem;
      border-radius: 4px;
      border: 1px solid #ccc;
      background: #f4f4f4;
      cursor: pointer;
      font-size: 0.8rem;
    }
    button:hover {
      background: #e2e2e2;
    }
    .messages {
      max-height: 150px;
      overflow-y: auto;
      border: 1px solid #eee;
      border-radius: 4px;
      padding: 0.4rem;
      background: #fafafa;
    }
    .message {
      margin-bottom: 0.25rem;
    }
    .message span.time {
      color: #666;
      margin-right: 0.25rem;
    }
    .steps {
      margin-top: 0.5rem;
      border-top: 1px solid #eee;
      padding-top: 0.5rem;
    }
    .steps ol {
      margin: 0;
      padding-left: 1.2rem;
    }
  </style>

  <h2>Messages & alertes</h2>
  <div class="toolbar">
    <button type="button" id="simulate">Simuler un message</button>
    <button type="button" id="recompute">Recalculer l'itinéraire</button>
  </div>
  <div class="messages"></div>
  <div class="steps">
    <strong>Étapes de l'itinéraire :</strong>
    <ol></ol>
  </div>
`;

export class Messages extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    const simulateBtn = this.shadowRoot.querySelector('#simulate');
    const recomputeBtn = this.shadowRoot.querySelector('#recompute');

    simulateBtn.addEventListener('click', () => this._simulateMessage());
    recomputeBtn.addEventListener('click', () => {
      this.addMessage('Recalcul d’itinéraire demandé (ex: plus de vélos à la station).');
      this.dispatchEvent(new CustomEvent('recompute-itinerary', {
        bubbles: true,
        composed: true
      }));
    });

    // 🔌 Ici, plus tard, tu pourras :
    // - ouvrir un WebSocket vers ActiveMQ (via STOMP)
    // - écouter une queue/topic
    // - appeler this.addMessage(...) à chaque message reçu
  }

  addMessage(text) {
    const container = this.shadowRoot.querySelector('.messages');
    const div = document.createElement('div');
    div.className = 'message';

    const time = new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });

    div.innerHTML = `<span class="time">[${time}]</span>${text}`;
    container.prepend(div); // le plus récent en haut
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

  _simulateMessage() {
    const samples = [
      "Averse prévue sur la fin du trajet.",
      "Pic de pollution : évitez les efforts trop intenses.",
      "Travaux détectés : un léger détour est conseillé.",
      "Plus que 2 vélos disponibles à la station de départ.",
      "Station d'arrivée presque pleine : risque de saturation."
    ];
    const msg = samples[Math.floor(Math.random() * samples.length)];
    this.addMessage(msg);
  }
}

customElements.define('compo-messages', Messages);
