import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { FiCheck, FiArrowRight, FiShoppingCart, FiBarChart2, FiPackage, FiSmartphone, FiStar, FiSun, FiMoon, FiPrinter } from 'react-icons/fi';
import { useTheme } from '../context/ThemeContext';

const features = [
  { icon: <FiShoppingCart />, title: 'Punto de Venta', desc: 'Carrito, codigo de barras y multiples metodos de pago.' },
  { icon: <FiPackage />, title: 'Stock Inteligente', desc: 'Unidades de medida, recetas y descuento automatico.' },
  { icon: <FiBarChart2 />, title: 'Reportes', desc: 'Ventas por dia, productos top, exportacion Excel.' },
  { icon: <FiSmartphone />, title: 'Alertas WhatsApp', desc: 'Stock bajo y resumen de caja en tu celular.' },
  { icon: <FiPrinter />, title: 'Impresion de Tickets', desc: 'Tickets y facturas en impresora termica 58mm.' },
];

const planes = [
  { nombre: 'Prueba Gratis', precio: '$0', periodo: '7 dias', features: ['Todas las funciones', '1 usuario', 'Datos en la nube', 'Soporte basico'], destacado: false, crypto: null, botonTexto: 'Empezar gratis' },
  { nombre: 'Mensual (Web)', precio: '$35', periodo: 'USD/mes', features: ['Acceso completo via web', 'Usuarios ilimitados', 'Soporte prioritario', 'Backup diario', 'Actualizaciones incluidas'], destacado: true, crypto: '29 USDT/mes', botonTexto: 'Elegir Mensual' },
  { nombre: 'De por vida (App)', precio: '$380', periodo: 'USD pago unico', features: ['App de escritorio para siempre', 'Sin pagos mensuales', 'Usuarios ilimitados', 'Soporte de por vida', 'Todas las actualizaciones'], destacado: false, crypto: '380 USDT pago unico', botonTexto: 'Comprar App' },
];

const testimonios = [
  { nombre: 'Carlos M.', negocio: 'Sandwicheria Don Pepe', texto: 'Desde que uso GastronomiApp, tengo todo organizado. Las alertas de stock me salvan la vida.', estrellas: 5 },
  { nombre: 'Laura G.', negocio: 'Pizzeria La Esquina', texto: 'Super facil de usar. Mis empleados aprendieron en 10 minutos. Los reportes son geniales.', estrellas: 5 },
  { nombre: 'Martin R.', negocio: 'Cafe Central', texto: 'El mejor sistema que probe. El control de recetas y descuento automatico es increible.', estrellas: 5 },
];

export default function Landing() {
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();

  return (
    <div className="lt-landing">
      {/* Navbar */}
      <header className="lt-navbar">
        <div className="lt-navbar-left">
          <img src="/logo.png" alt="GastronomiApp" className="lt-navbar-logo" />
          <span className="lt-navbar-brand">GastronomiApp</span>
        </div>
        <nav className="lt-navbar-links">
          <a href="#features">Funciones</a>
          <a href="#pricing">Planes</a>
          <a href="#testimonials">Testimonios</a>
        </nav>
        <div className="lt-navbar-right">
          <button className="lt-theme-toggle" onClick={toggleTheme} title={theme === 'light' ? 'Modo oscuro' : 'Modo claro'}>
            {theme === 'light' ? <FiMoon /> : <FiSun />}
          </button>
          <button className="lt-btn-ghost" onClick={() => navigate('/login')}>Iniciar Sesion</button>
          <button className="lt-btn-primary" onClick={() => navigate('/registro')}>Probar Gratis</button>
        </div>
      </header>

      {/* Hero */}
      <section className="lt-hero">
        <motion.div className="lt-hero-text" initial={{ opacity: 0, y: 30 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
          <h1 className="lt-hero-title">Gestion gastronomica<br />simple y poderosa</h1>
          <p className="lt-hero-subtitle">Todo lo que necesitas para manejar tu negocio: ventas, stock, recetas, reportes y mas.</p>
          <div className="lt-hero-actions">
            <button className="lt-btn-primary lt-btn-lg" onClick={() => navigate('/registro')}>
              Empezar gratis <FiArrowRight />
            </button>
            <button className="lt-btn-outline" onClick={() => navigate('/login')}>
              Ya tengo cuenta
            </button>
          </div>
          <p className="lt-hero-note">7 dias gratis. Sin tarjeta de credito.</p>
        </motion.div>

        <motion.div className="lt-hero-image" initial={{ opacity: 0, y: 40 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.7, delay: 0.2 }}>
          <img src="/hero-food.png" alt="GastronomiApp Dashboard" className="lt-hero-img" />
        </motion.div>

        <FiStar className="lt-sparkle lt-sparkle-1" />
        <FiStar className="lt-sparkle lt-sparkle-2" />
      </section>

      {/* Trust Bar */}
      <div className="lt-trust-bar">
        <span>Punto de Venta</span>
        <div className="lt-trust-dot" />
        <span>Control de Stock</span>
        <div className="lt-trust-dot" />
        <span>Recetas</span>
        <div className="lt-trust-dot" />
        <span>Reportes</span>
        <div className="lt-trust-dot" />
        <span>WhatsApp</span>
      </div>

      {/* Features */}
      <section className="lt-features" id="features">
        <div className="lt-section-header">
          <h2 className="lt-section-title">Todo lo que necesitas</h2>
          <p className="lt-section-subtitle">Herramientas pensadas para gastronomia</p>
        </div>
        <div className="lt-features-grid">
          {features.map((f, i) => (
            <motion.div key={i} className="lt-feature-card" initial={{ opacity: 0, y: 20 }} whileInView={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }} viewport={{ once: true }}>
              <div className="lt-feature-icon">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.desc}</p>
            </motion.div>
          ))}
        </div>
      </section>

      {/* Pricing */}
      <section className="lt-pricing" id="pricing">
        <div className="lt-section-header">
          <h2 className="lt-section-title">Planes simples</h2>
          <p className="lt-section-subtitle">Sin letra chica. Cancela cuando quieras.</p>
        </div>
        <div className="lt-pricing-grid">
          {planes.map((plan, i) => (
            <motion.div key={i} className={`lt-pricing-card ${plan.destacado ? 'lt-pricing-featured' : ''}`} initial={{ opacity: 0, y: 20 }} whileInView={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }} viewport={{ once: true }}>
              {plan.destacado && <div className="lt-pricing-badge">Popular</div>}
              <div className="lt-pricing-name">{plan.nombre}</div>
              <div className="lt-pricing-price">
                <span className="lt-price-amount">{plan.precio}</span>
                <span className="lt-price-period">{plan.periodo}</span>
              </div>
              {plan.crypto && <div className="lt-pricing-crypto">o {plan.crypto}</div>}
              <ul className="lt-pricing-features">
                {plan.features.map((f, j) => <li key={j}><FiCheck /> {f}</li>)}
              </ul>
              <button className={plan.destacado ? 'lt-btn-primary lt-btn-full' : 'lt-btn-outline lt-btn-full'} onClick={() => navigate('/registro')}>{plan.botonTexto}</button>
            </motion.div>
          ))}
        </div>
      </section>

      {/* Testimonials */}
      <section className="lt-testimonials" id="testimonials">
        <div className="lt-section-header">
          <h2 className="lt-section-title">Lo que dicen nuestros clientes</h2>
          <p className="lt-section-subtitle">Negocios reales usando GastronomiApp</p>
        </div>
        <div className="lt-testimonials-grid">
          {testimonios.map((t, i) => (
            <motion.div key={i} className="lt-testimonial-card" initial={{ opacity: 0, y: 20 }} whileInView={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }} viewport={{ once: true }}>
              <div className="lt-testimonial-stars">
                {Array.from({ length: t.estrellas }).map((_, j) => <FiStar key={j} />)}
              </div>
              <p className="lt-testimonial-text">"{t.texto}"</p>
              <div className="lt-testimonial-author">
                <strong>{t.nombre}</strong>
                <span>{t.negocio}</span>
              </div>
            </motion.div>
          ))}
        </div>
      </section>

      {/* CTA */}
      <section className="lt-cta">
        <motion.div className="lt-cta-content" initial={{ opacity: 0, scale: 0.95 }} whileInView={{ opacity: 1, scale: 1 }} viewport={{ once: true }}>
          <h2>Listo para empezar?</h2>
          <p>Proba GastronomiApp gratis por 7 dias y lleva tu negocio al siguiente nivel.</p>
          <button className="lt-btn-primary lt-btn-xl" onClick={() => navigate('/registro')}>
            Crear mi cuenta gratis <FiArrowRight />
          </button>
        </motion.div>
      </section>

      {/* Footer */}
      <footer className="lt-footer">
        <div className="lt-footer-left">
          <img src="/logo.png" alt="GastronomiApp" className="lt-footer-logo" />
          <span>GastronomiApp &copy; 2026</span>
        </div>
        <div className="lt-footer-right">
          admin@gastronomiapp.com
        </div>
      </footer>
    </div>
  );
}
