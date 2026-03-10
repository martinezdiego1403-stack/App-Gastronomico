import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { FiCheck, FiArrowRight, FiShoppingCart, FiBarChart2, FiPackage, FiSmartphone, FiStar } from 'react-icons/fi';

// ============================================
// DATA
// ============================================

const features = [
  { icon: <FiShoppingCart />, title: 'Punto de Venta', desc: 'Vende rapido con carrito, codigo de barras y multiples metodos de pago.' },
  { icon: <FiPackage />, title: 'Stock Inteligente', desc: 'Control de mercaderia con unidades de medida, recetas y descuento automatico.' },
  { icon: <FiBarChart2 />, title: 'Reportes', desc: 'Ventas por dia, productos mas vendidos, exportacion a Excel.' },
  { icon: <FiSmartphone />, title: 'Alertas WhatsApp', desc: 'Recibi alertas de stock bajo y resumen de caja en tu celular.' },
];

const planes = [
  {
    nombre: 'Prueba Gratis',
    precio: '$0',
    periodo: '7 dias',
    features: ['Todas las funciones', '1 usuario', 'Datos en la nube', 'Soporte basico'],
    destacado: false,
    crypto: null,
    botonTexto: 'Empezar gratis',
  },
  {
    nombre: 'Mensual (Web)',
    precio: '$35',
    periodo: 'USD/mes',
    features: ['Acceso completo via web', 'Usuarios ilimitados', 'Soporte prioritario', 'Backup diario', 'Actualizaciones incluidas'],
    destacado: true,
    crypto: '29 USDT/mes',
    botonTexto: 'Elegir Mensual',
  },
  {
    nombre: 'De por vida (App)',
    precio: '$380',
    periodo: 'USD pago unico',
    features: ['App de escritorio para siempre', 'Sin pagos mensuales', 'Usuarios ilimitados', 'Soporte de por vida', 'Todas las actualizaciones'],
    destacado: false,
    crypto: '380 USDT pago unico',
    botonTexto: 'Comprar App',
  },
];

const testimonials = [
  { name: 'Carlos M.', role: 'Sandwicheria Don Pepe', text: 'Desde que uso GastronomiApp mis ventas estan organizadas y el stock se controla solo.' },
  { name: 'Laura S.', role: 'Pizzeria La Nona', text: 'Las recetas se descuentan automaticamente del inventario. Me ahorra horas de trabajo.' },
  { name: 'Diego R.', role: 'Bar El Clasico', text: 'Los reportes me ayudan a saber que productos rinden mas. Muy recomendable.' },
];

// ============================================
// MAIN LANDING
// ============================================

export default function Landing() {
  const navigate = useNavigate();

  return (
    <div className="lt-landing">
      {/* ===== NAVBAR ===== */}
      <header className="lt-navbar">
        <div className="lt-navbar-left">
          <img src="/logo.png" alt="GastronomiApp" className="lt-navbar-logo" />
          <span className="lt-navbar-brand">GastronomiApp</span>
        </div>
        <nav className="lt-navbar-links">
          <a href="#features">Funciones</a>
          <a href="#pricing">Planes</a>
          <a href="#testimonials">Opiniones</a>
        </nav>
        <div className="lt-navbar-right">
          <button className="lt-btn-ghost" onClick={() => navigate('/login')}>Iniciar Sesion</button>
          <button className="lt-btn-primary" onClick={() => navigate('/registro')}>
            Probar Gratis <FiArrowRight />
          </button>
        </div>
      </header>

      {/* ===== HERO SECTION ===== */}
      <section className="lt-hero">
        <motion.div
          className="lt-hero-text"
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.7 }}
        >
          <h1 className="lt-hero-title">GESTION GASTRONOMICA.</h1>
          <p className="lt-hero-subtitle">Tu negocio bajo control, simple y rapido</p>
          <div className="lt-hero-actions">
            <button className="lt-btn-primary lt-btn-lg" onClick={() => navigate('/registro')}>
              Empezar gratis por 7 dias <FiArrowRight />
            </button>
            <button className="lt-btn-outline" onClick={() => navigate('/login')}>
              Ya tengo cuenta
            </button>
          </div>
          <p className="lt-hero-note">Sin tarjeta de credito requerida</p>
        </motion.div>

        <motion.div
          className="lt-hero-image"
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.8, delay: 0.2 }}
        >
          <img src="/hero-food.png" alt="Comida gourmet - Pizza, Hamburguesas, Batidos" className="lt-hero-img" />
        </motion.div>

        {/* Sparkle decoration */}
        <div className="lt-sparkle lt-sparkle-1"><FiStar /></div>
        <div className="lt-sparkle lt-sparkle-2"><FiStar /></div>
      </section>

      {/* ===== BRANDS/TRUST BAR ===== */}
      <section className="lt-trust-bar">
        <span>Punto de Venta</span>
        <span className="lt-trust-dot" />
        <span>Control de Stock</span>
        <span className="lt-trust-dot" />
        <span>Facturacion</span>
        <span className="lt-trust-dot" />
        <span>Reportes</span>
        <span className="lt-trust-dot" />
        <span>Multi-sucursal</span>
      </section>

      {/* ===== FEATURES ===== */}
      <section className="lt-features" id="features">
        <motion.div
          className="lt-section-header"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <h2 className="lt-section-title">Todo lo que necesitas</h2>
          <p className="lt-section-subtitle">Herramientas profesionales para tu negocio gastronomico</p>
        </motion.div>

        <div className="lt-features-grid">
          {features.map((f, i) => (
            <motion.div
              key={i}
              className="lt-feature-card"
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1, duration: 0.5 }}
              viewport={{ once: true }}
            >
              <div className="lt-feature-icon">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.desc}</p>
            </motion.div>
          ))}
        </div>
      </section>

      {/* ===== PRICING ===== */}
      <section className="lt-pricing" id="pricing">
        <motion.div
          className="lt-section-header"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <h2 className="lt-section-title">Planes simples, sin sorpresas</h2>
          <p className="lt-section-subtitle">Elegi el plan que mejor se adapte a tu negocio</p>
        </motion.div>

        <div className="lt-pricing-grid">
          {planes.map((plan, i) => (
            <motion.div
              key={i}
              className={`lt-pricing-card ${plan.destacado ? 'lt-pricing-featured' : ''}`}
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.12, duration: 0.5 }}
              viewport={{ once: true }}
            >
              {plan.destacado && <div className="lt-pricing-badge">Mas popular</div>}
              <h3 className="lt-pricing-name">{plan.nombre}</h3>
              <div className="lt-pricing-price">
                <span className="lt-price-amount">{plan.precio}</span>
                <span className="lt-price-period">{plan.periodo}</span>
              </div>
              {plan.crypto && <div className="lt-pricing-crypto">o {plan.crypto}</div>}
              <ul className="lt-pricing-features">
                {plan.features.map((f, j) => (
                  <li key={j}><FiCheck /> {f}</li>
                ))}
              </ul>
              <button
                className={plan.destacado ? 'lt-btn-primary lt-btn-full' : 'lt-btn-outline lt-btn-full'}
                onClick={() => navigate('/registro')}
              >
                {plan.botonTexto}
              </button>
            </motion.div>
          ))}
        </div>
      </section>

      {/* ===== TESTIMONIALS ===== */}
      <section className="lt-testimonials" id="testimonials">
        <motion.div
          className="lt-section-header"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <h2 className="lt-section-title">Lo que dicen nuestros clientes</h2>
        </motion.div>

        <div className="lt-testimonials-grid">
          {testimonials.map((t, i) => (
            <motion.div
              key={i}
              className="lt-testimonial-card"
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              viewport={{ once: true }}
            >
              <div className="lt-testimonial-stars">
                {[...Array(5)].map((_, j) => <FiStar key={j} />)}
              </div>
              <p className="lt-testimonial-text">"{t.text}"</p>
              <div className="lt-testimonial-author">
                <strong>{t.name}</strong>
                <span>{t.role}</span>
              </div>
            </motion.div>
          ))}
        </div>
      </section>

      {/* ===== CTA ===== */}
      <section className="lt-cta">
        <motion.div
          className="lt-cta-content"
          initial={{ opacity: 0, scale: 0.95 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
        >
          <h2>Listo para empezar?</h2>
          <p>Proba GastronomiApp gratis por 7 dias y lleva tu negocio al siguiente nivel.</p>
          <button className="lt-btn-primary lt-btn-xl" onClick={() => navigate('/registro')}>
            Crear mi cuenta gratis <FiArrowRight />
          </button>
        </motion.div>
      </section>

      {/* ===== FOOTER ===== */}
      <footer className="lt-footer">
        <div className="lt-footer-left">
          <img src="/logo.png" alt="GastronomiApp" className="lt-footer-logo" />
          <span>GastronomiApp &copy; 2026</span>
        </div>
        <div className="lt-footer-right">
          <span>admin@gastronomiapp.com</span>
        </div>
      </footer>
    </div>
  );
}
