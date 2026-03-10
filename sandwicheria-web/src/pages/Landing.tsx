import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { FiCheck, FiArrowRight, FiShoppingCart, FiBarChart2, FiPackage, FiSmartphone } from 'react-icons/fi';

const features = [
  { icon: <FiShoppingCart />, title: 'Punto de Venta', desc: 'Vende rapido con carrito, codigo de barras y multiples metodos de pago.' },
  { icon: <FiPackage />, title: 'Stock Inteligente', desc: 'Control de mercaderia con unidades de medida, recetas y descuento automatico.' },
  { icon: <FiBarChart2 />, title: 'Reportes', desc: 'Ventas por dia, productos mas vendidos, exportacion a Excel.' },
  { icon: <FiSmartphone />, title: 'Alertas WhatsApp', desc: 'Recibí alertas de stock bajo y resumen de caja en tu celular.' },
];

const planes = [
  {
    nombre: 'Prueba Gratis',
    precio: '$0',
    periodo: '7 dias',
    features: ['Todas las funciones', '1 usuario', 'Datos en la nube', 'Soporte basico'],
    destacado: false,
  },
  {
    nombre: 'Pro',
    precio: '$14.999',
    periodo: '/mes',
    features: ['Todas las funciones', 'Usuarios ilimitados', 'App de escritorio', 'Soporte prioritario', 'Backup diario'],
    destacado: true,
  },
];

export default function Landing() {
  const navigate = useNavigate();

  return (
    <div className="landing-page">
      {/* Header */}
      <header className="landing-header">
        <div className="landing-header-left">
          <img src="/logo.png" alt="GastronomiApp" className="header-logo-img" />
          <span className="landing-brand">GastronomiApp</span>
        </div>
        <div className="landing-header-right">
          <button className="btn-ghost" onClick={() => navigate('/login')}>
            Iniciar Sesion
          </button>
          <button className="btn-primary" onClick={() => navigate('/registro')}>
            Probar Gratis
          </button>
        </div>
      </header>

      {/* Hero */}
      <section className="landing-hero">
        <motion.div
          className="hero-content"
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
        >
          <h1 className="hero-title">
            Tu negocio gastronomico,
            <br />
            <span className="text-accent">bajo control total.</span>
          </h1>
          <p className="hero-subtitle">
            Sistema de gestion completo para sandwicherias, restaurantes y
            locales de comida. Ventas, stock, reportes y mas.
          </p>
          <div className="hero-buttons">
            <button className="btn-primary btn-lg" onClick={() => navigate('/registro')}>
              Empezar gratis por 7 dias
              <FiArrowRight />
            </button>
          </div>
          <p className="hero-note">Sin tarjeta de credito. Cancela cuando quieras.</p>
        </motion.div>
      </section>

      {/* Features */}
      <section className="landing-features" id="features">
        <h2 className="section-title">Todo lo que necesitas</h2>
        <div className="features-grid">
          {features.map((f, i) => (
            <motion.div
              key={i}
              className="feature-card"
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              viewport={{ once: true }}
            >
              <div className="feature-icon">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.desc}</p>
            </motion.div>
          ))}
        </div>
      </section>

      {/* Pricing */}
      <section className="landing-pricing" id="pricing">
        <h2 className="section-title">Planes simples</h2>
        <div className="pricing-grid">
          {planes.map((plan, i) => (
            <motion.div
              key={i}
              className={`pricing-card ${plan.destacado ? 'pricing-featured' : ''}`}
              initial={{ opacity: 0, scale: 0.95 }}
              whileInView={{ opacity: 1, scale: 1 }}
              transition={{ delay: i * 0.15 }}
              viewport={{ once: true }}
            >
              <h3 className="pricing-name">{plan.nombre}</h3>
              <div className="pricing-price">
                <span className="price-amount">{plan.precio}</span>
                <span className="price-period">{plan.periodo}</span>
              </div>
              <ul className="pricing-features">
                {plan.features.map((f, j) => (
                  <li key={j}><FiCheck /> {f}</li>
                ))}
              </ul>
              <button
                className={plan.destacado ? 'btn-primary' : 'btn-ghost'}
                onClick={() => navigate('/registro')}
              >
                {plan.destacado ? 'Elegir Pro' : 'Empezar gratis'}
              </button>
            </motion.div>
          ))}
        </div>
      </section>

      {/* Footer */}
      <footer className="landing-footer">
        <span>GastronomiApp &copy; 2026 - Sistema de Gestion Gastronomica</span>
      </footer>
    </div>
  );
}
