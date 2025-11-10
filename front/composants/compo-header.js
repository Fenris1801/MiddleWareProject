const template = document.createElement('template');
template.innerHTML = `
  <header class="site-header">
    <a class="logo" href="#/"> Let’s Go Biking</a>
    <nav class="links">
      <a href="#/">Accueil</a>
      <a href="#/itinerary">Itinéraire</a>
      <a href="#/about">À propos</a>
    </nav>
    <button class="burger" aria-label="Ouvrir le menu" aria-expanded="false">
      <span></span><span></span><span></span>
    </button>
  </header>
`;

class compoHeader extends HTMLElement {
  constructor() {
    super();
    const root = this.attachShadow({ mode: 'open' });
    root.append(template.content.cloneNode(true));
  }
}

customElements.define('compo-header', compoHeader);
export { compoHeader };
