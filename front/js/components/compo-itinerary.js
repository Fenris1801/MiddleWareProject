const template = document.createElement('template');
template.innerHTML = `
   <style>
    :host {
      display: block;
      font-size: 0.9rem;
      color: #111827;
    }

    form {
      display: flex;
      align-items: stretch;
      gap: 0.75rem;
    }

    .field-row {
      display: flex;
      align-items: flex-start; 
      gap: 0.75rem;
      width: 100%;
    }

    .field-group {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      flex: 1 1 0;
    }

    .field-label {
      font-size: 0.75rem;
      font-weight: 500;
      color: #454952ff;
    }

    .field-input {
      padding: 0.7rem 0.9rem;
      border-radius: 999px;
      border: none;
      background: #f3f4f6;
      font-size: 0.9rem;
      outline: none;
    }

    .field-input::placeholder {
      color: #9ca3af;
    }

    .field-input:focus {
      box-shadow: 0 0 0 2px rgba(59,130,246,.35);
      background: #ffffff;
    }

    .options {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      white-space: nowrap;
    }

    .checkbox {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.85rem;
      color: #4b5563;
    }

    .checkbox input[type="checkbox"] {
      width: 14px;
      height: 14px;
      accent-color: #2563eb;
      cursor: pointer;
    }

    .btn {
      border-radius: 999px;
      padding: 0.6rem 1.1rem;
      font-size: 0.85rem;
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: background 0.15s ease, box-shadow 0.15s ease, transform 0.05s;
      white-space: nowrap;
    }

    .btn:active {
      transform: translateY(1px);
    }

    .btn-ghost {
      background: #f3f4f6;
      color: #4b5563;
    }

    .btn-ghost:hover {
      background: #e5e7eb;
    }

    .btn-primary {
    margin-top: 1.4rem;
      background: #3b82f6;
      color: #ffffff;
      box-shadow: 0 4px 10px rgba(59,130,246,0.35);
    }

    .btn-primary:hover {
      background: #2563eb;
    }
      
    
    .suggestions,
  .suggestion-item,
  .suggestion-item * {
    cursor: pointer;
}
  </style>

  <form>
    <div class="field-row">

      <div class="field-group">
        <span class="field-label">Adresse de départ</span>
        <input
          class="field-input"
          name="origin"
          type="text"
          placeholder="Entrez l'adresse de départ"
          autocomplete="off"
          required
        />
        
      <ul class="suggestions hidden" data-for="origin"></ul>
      </div>

      <div class="field-group">
        <span class="field-label">Adresse d'arrivée</span>
        <input
          class="field-input"
          name="destination"
          type="text"
          placeholder="Entrez l'adresse d'arrivée"
          autocomplete="off"
          required
        />

      <ul class="suggestions hidden" data-for="destination"></ul>
      </div>

      <button type="submit" class="btn btn-primary">
        Rechercher
      </button>
    </div>
  </form>
`;

export class Itinerary extends HTMLElement {
  #lastQuery = null;
  #originInput;
  #destinationInput;
  #originSuggestions;
  #destinationSuggestions;
  #originTimeout = null;
  #destinationTimeout = null;

  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    const form = this.shadowRoot.querySelector('form');
    this.#originInput = form.elements.namedItem('origin');
    this.#destinationInput = form.elements.namedItem('destination');
    this.#originSuggestions = this.shadowRoot.querySelector('.suggestions[data-for="origin"]');
    this.#destinationSuggestions = this.shadowRoot.querySelector('.suggestions[data-for="destination"]');

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

    this.#originInput.addEventListener('input', () => {
      this.#debouncedFetchSuggestions(
        this.#originInput,
        this.#originSuggestions,
        'origin'
      );
    });


    // Autocomplétion pour la destination
    this.#destinationInput.addEventListener('input', () => {
      this.#debouncedFetchSuggestions(
        this.#destinationInput,
        this.#destinationSuggestions,
        'destination'
      );
    });

    this.#originInput.addEventListener('blur', () => {
      setTimeout(() => this.#clearSuggestions(this.#originSuggestions), 150);
    });
    this.#destinationInput.addEventListener('blur', () => {
      setTimeout(() => this.#clearSuggestions(this.#destinationSuggestions), 150);
    });
  }

  getLastQuery() {
    return this.#lastQuery;
  }

  #debouncedFetchSuggestions(inputEl, listEl, type) {
    const value = inputEl.value.trim();

    if (value.length < 3) {
      this.#clearSuggestions(listEl);
      return;
    }

    const timeoutProp = type === 'origin' ? '#originTimeout' : '#destinationTimeout';
    if (this[timeoutProp]) {
      clearTimeout(this[timeoutProp]);
    }

    this[timeoutProp] = setTimeout(() => {
      this.#fetchSuggestions(value)
        .then((features) => {
          this.#renderSuggestions(listEl, inputEl, features);
        })
        .catch((err) => {
          console.error('Erreur API adresse:', err);
          this.#clearSuggestions(listEl);
        });
    }, 250);
  }

  async #fetchSuggestions(query) {
    const url = new URL('https://api-adresse.data.gouv.fr/search/');
    url.searchParams.set('q', query);
    url.searchParams.set('limit', '5');
    url.searchParams.set('autocomplete', '1');

    const res = await fetch(url.toString());
    if (!res.ok) {
      throw new Error('HTTP ' + res.status);
    }
    const data = await res.json();
    return data.features || [];
  }

  #renderSuggestions(listEl, inputEl, features) {
    this.#clearSuggestions(listEl);

    if (!features.length) {
      return;
    }

    listEl.classList.remove('hidden');

    features.forEach((f) => {
      const props = f.properties || {};
      const geom = f.geometry || {};
      const coords = geom.coordinates || [];
      const lon = coords[0];
      const lat = coords[1];

      const li = document.createElement('li');
      li.className = 'suggestion-item';

      const main = document.createElement('span');
      main.className = 'suggestion-main';
      main.textContent = props.label || '';

      const sub = document.createElement('span');
      sub.className = 'suggestion-sub';
      const parts = [props.postcode, props.city].filter(Boolean);
      sub.textContent = parts.join(' · ');

      li.appendChild(main);
      if (sub.textContent) {
        li.appendChild(sub);
      }

      // Enregistrer les coordonnées sur l'élément, si tu veux les récupérer plus tard
      li.dataset.label = props.label || '';
      if (lat != null && lon != null) {
        li.dataset.lat = lat;
        li.dataset.lon = lon;
      }

      li.addEventListener('mousedown', (e) => {
        e.preventDefault();
        inputEl.value = li.dataset.label;
        if (li.dataset.lat && li.dataset.lon) {
          inputEl.dataset.lat = li.dataset.lat;
          inputEl.dataset.lon = li.dataset.lon;
        }
        this.#clearSuggestions(listEl);
      });

      listEl.appendChild(li);
    });
  }

  #clearSuggestions(listEl) {
    listEl.innerHTML = '';
    listEl.classList.add('hidden');
  }
}

customElements.define('compo-itinerary', Itinerary);
