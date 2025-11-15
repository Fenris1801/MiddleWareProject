const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      display: block;
      font-size: 0.9rem;
    }
    h2 {
      margin: 0 0 0.5rem;
      font-size: 1rem;
    }
    form {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    label {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
    input {
      padding: 0.35rem 0.5rem;
      border-radius: 4px;
      border: 1px solid #ccc;
      font-size: 0.9rem;
    }
    button {
      margin-top: 0.5rem;
      padding: 0.4rem 0.6rem;
      border-radius: 4px;
      border: none;
      background: #007bff;
      color: white;
      cursor: pointer;
      font-size: 0.9rem;
    }
    button:hover {
      background: #005fcc;
    }
  </style>

  <h2>Itinéraire</h2>
  <form>
    <label>
      Origine
      <input name="origin" type="text" placeholder="Adresse, station, etc." required />
    </label>
    <label>
      Destination
      <input name="destination" type="text" placeholder="Adresse, station, etc." required />
    </label>
    <button type="submit">Calculer l'itinéraire</button>
  </form>
`;

export class Itinerary extends HTMLElement {
  #lastQuery = null;

  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    const form = this.shadowRoot.querySelector('form');
    form.addEventListener('submit', (e) => {
      e.preventDefault();
      const origin = form.origin.value.trim();
      const destination = form.destination.value.trim();
      if (!origin || !destination) return;

      this.#lastQuery = { origin, destination };

      this.dispatchEvent(new CustomEvent('request-itinerary', {
        bubbles: true,
        composed: true,
        detail: { origin, destination }
      }));
    });
  }

  getLastQuery() {
    return this.#lastQuery;
  }
}

customElements.define('compo-itinerary', Itinerary);
