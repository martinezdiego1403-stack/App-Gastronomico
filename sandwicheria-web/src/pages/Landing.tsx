import { useRef, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, useScroll, useTransform } from 'framer-motion';
import { Canvas, useFrame } from '@react-three/fiber';
import { Float, MeshDistortMaterial, MeshWobbleMaterial, Stars, Environment } from '@react-three/drei';
import { FiCheck, FiArrowRight, FiShoppingCart, FiBarChart2, FiPackage, FiSmartphone } from 'react-icons/fi';
import * as THREE from 'three';

// ============================================
// 3D SCENE COMPONENTS
// ============================================

function FloatingOrb({ position, color, speed = 1, distort = 0.4, size = 1 }: any) {
  const meshRef = useRef<THREE.Mesh>(null!);
  useFrame((state) => {
    if (meshRef.current) {
      meshRef.current.rotation.x = state.clock.elapsedTime * 0.2 * speed;
      meshRef.current.rotation.y = state.clock.elapsedTime * 0.3 * speed;
    }
  });
  return (
    <Float speed={speed * 2} rotationIntensity={0.5} floatIntensity={1.5}>
      <mesh ref={meshRef} position={position} scale={size}>
        <icosahedronGeometry args={[1, 4]} />
        <MeshDistortMaterial
          color={color}
          distort={distort}
          speed={2}
          roughness={0.2}
          metalness={0.8}
        />
      </mesh>
    </Float>
  );
}

function WobblySphere({ position, color, size = 0.6 }: any) {
  return (
    <Float speed={3} rotationIntensity={1} floatIntensity={2}>
      <mesh position={position} scale={size}>
        <sphereGeometry args={[1, 32, 32]} />
        <MeshWobbleMaterial
          color={color}
          factor={0.6}
          speed={2}
          transparent
          opacity={0.7}
        />
      </mesh>
    </Float>
  );
}

function FloatingRing({ position, color, size = 1 }: any) {
  const meshRef = useRef<THREE.Mesh>(null!);
  useFrame((state) => {
    if (meshRef.current) {
      meshRef.current.rotation.x = state.clock.elapsedTime * 0.5;
      meshRef.current.rotation.z = state.clock.elapsedTime * 0.3;
    }
  });
  return (
    <Float speed={2} floatIntensity={1}>
      <mesh ref={meshRef} position={position} scale={size}>
        <torusGeometry args={[1, 0.3, 16, 32]} />
        <meshStandardMaterial color={color} metalness={0.9} roughness={0.1} />
      </mesh>
    </Float>
  );
}

function HeroScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 8], fov: 60 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true, antialias: true }}
    >
      <ambientLight intensity={0.3} />
      <directionalLight position={[5, 5, 5]} intensity={0.8} color="#ffa726" />
      <pointLight position={[-5, -3, 3]} intensity={0.6} color="#ff6b35" />
      <pointLight position={[3, 4, -3]} intensity={0.4} color="#448aff" />

      <Stars radius={80} depth={60} count={1500} factor={3} saturation={0.2} fade speed={0.5} />

      <FloatingOrb position={[-3.5, 1.5, 0]} color="#ff6b35" speed={0.8} distort={0.5} size={1.2} />
      <FloatingOrb position={[3, -1, -2]} color="#ffa726" speed={1.2} distort={0.3} size={0.8} />
      <FloatingOrb position={[0, 2.5, -3]} color="#448aff" speed={0.6} distort={0.4} size={0.6} />

      <WobblySphere position={[4, 2, -1]} color="#ff6b35" size={0.5} />
      <WobblySphere position={[-2, -2, -2]} color="#ffa726" size={0.7} />

      <FloatingRing position={[-4, -1, -3]} color="#ff6b35" size={0.5} />
      <FloatingRing position={[2, 3, -4]} color="#448aff" size={0.4} />

      <Environment preset="night" />
    </Canvas>
  );
}

function FeaturesScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 6], fov: 50 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.2} />
      <pointLight position={[3, 3, 3]} intensity={0.8} color="#ff6b35" />

      <Float speed={1.5} rotationIntensity={0.3} floatIntensity={1}>
        <mesh position={[3, -2, -2]} scale={1.5}>
          <octahedronGeometry args={[1, 0]} />
          <meshStandardMaterial color="#ff6b35" metalness={0.7} roughness={0.2} wireframe />
        </mesh>
      </Float>

      <Float speed={2} rotationIntensity={0.5} floatIntensity={2}>
        <mesh position={[-3, 2, -3]} scale={0.8}>
          <dodecahedronGeometry args={[1, 0]} />
          <meshStandardMaterial color="#ffa726" metalness={0.8} roughness={0.1} wireframe />
        </mesh>
      </Float>

      <Stars radius={50} depth={30} count={500} factor={2} saturation={0} fade speed={0.3} />
    </Canvas>
  );
}

function PricingScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 7], fov: 50 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.2} />
      <pointLight position={[-3, 4, 3]} intensity={1} color="#ffa726" />
      <pointLight position={[3, -2, 2]} intensity={0.5} color="#448aff" />

      <FloatingOrb position={[-4, -2, -2]} color="#ffa726" speed={0.5} distort={0.6} size={1.5} />
      <FloatingRing position={[4, 2, -3]} color="#ff6b35" size={0.7} />
      <WobblySphere position={[0, -3, -4]} color="#448aff" size={1} />

      <Stars radius={60} depth={40} count={800} factor={3} saturation={0.1} fade speed={0.4} />
    </Canvas>
  );
}

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

// ============================================
// MAIN LANDING COMPONENT
// ============================================

export default function Landing() {
  const navigate = useNavigate();
  const containerRef = useRef<HTMLDivElement>(null);

  // Horizontal scroll: vertical scroll → horizontal translation
  const { scrollYProgress } = useScroll({ target: containerRef });
  const x = useTransform(scrollYProgress, [0, 1], ['0%', '-75%']);

  // Parallax for decorative elements
  const bgParallax = useTransform(scrollYProgress, [0, 1], ['0%', '-20%']);

  return (
    <div className="hz-landing" ref={containerRef}>
      {/* Fixed navbar */}
      <header className="hz-navbar">
        <div className="hz-navbar-left">
          <img src="/logo.png" alt="GastronomiApp" className="hz-navbar-logo" />
          <span className="hz-navbar-brand">GastronomiApp</span>
        </div>
        <nav className="hz-navbar-links">
          <a href="#features" onClick={e => { e.preventDefault(); containerRef.current?.scrollTo({ top: containerRef.current.scrollHeight * 0.25, behavior: 'smooth' }); }}>Funciones</a>
          <a href="#pricing" onClick={e => { e.preventDefault(); containerRef.current?.scrollTo({ top: containerRef.current.scrollHeight * 0.55, behavior: 'smooth' }); }}>Planes</a>
        </nav>
        <div className="hz-navbar-right">
          <button className="hz-btn-ghost" onClick={() => navigate('/login')}>Iniciar Sesion</button>
          <button className="hz-btn-primary" onClick={() => navigate('/registro')}>Probar Gratis</button>
        </div>
      </header>

      {/* Sticky horizontal container */}
      <div className="hz-sticky-wrap">
        <motion.div className="hz-panels" style={{ x }}>

          {/* PANEL 1: Hero */}
          <section className="hz-panel hz-panel-hero">
            <HeroScene />
            <div className="hz-hero-content">
              <motion.div
                initial={{ opacity: 0, x: -60 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.8 }}
              >
                <h1 className="hz-hero-title">
                  Tu negocio<br />gastronomico,<br />
                  <span className="hz-text-accent">bajo control total.</span>
                </h1>
                <p className="hz-hero-subtitle">
                  Sistema de gestion completo para sandwicherias,<br />
                  restaurantes y locales de comida.
                </p>
                <div className="hz-hero-actions">
                  <button className="hz-btn-primary hz-btn-lg" onClick={() => navigate('/registro')}>
                    Empezar gratis <FiArrowRight />
                  </button>
                  <span className="hz-hero-note">7 dias gratis. Sin tarjeta.</span>
                </div>
              </motion.div>
            </div>
            <div className="hz-scroll-indicator">
              <motion.div
                animate={{ y: [0, 10, 0] }}
                transition={{ repeat: Infinity, duration: 1.5 }}
                className="hz-scroll-arrow"
              >
                <FiArrowRight />
              </motion.div>
              <span>Desliza para explorar</span>
            </div>
          </section>

          {/* PANEL 2: Features */}
          <section className="hz-panel hz-panel-features" id="features">
            <FeaturesScene />
            <div className="hz-features-content">
              <motion.h2
                className="hz-section-title"
                initial={{ opacity: 0 }}
                whileInView={{ opacity: 1 }}
                viewport={{ once: true }}
              >
                Todo lo que necesitas
              </motion.h2>
              <div className="hz-features-grid">
                {features.map((f, i) => (
                  <motion.div
                    key={i}
                    className="hz-feature-card"
                    initial={{ opacity: 0, y: 40 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    transition={{ delay: i * 0.15, duration: 0.5 }}
                    viewport={{ once: true }}
                  >
                    <div className="hz-feature-icon">{f.icon}</div>
                    <h3>{f.title}</h3>
                    <p>{f.desc}</p>
                  </motion.div>
                ))}
              </div>
            </div>
          </section>

          {/* PANEL 3: Pricing */}
          <section className="hz-panel hz-panel-pricing" id="pricing">
            <PricingScene />
            <div className="hz-pricing-content">
              <motion.h2
                className="hz-section-title"
                initial={{ opacity: 0 }}
                whileInView={{ opacity: 1 }}
                viewport={{ once: true }}
              >
                Planes simples
              </motion.h2>
              <div className="hz-pricing-grid">
                {planes.map((plan, i) => (
                  <motion.div
                    key={i}
                    className={`hz-pricing-card ${plan.destacado ? 'hz-pricing-featured' : ''}`}
                    initial={{ opacity: 0, scale: 0.9 }}
                    whileInView={{ opacity: 1, scale: 1 }}
                    transition={{ delay: i * 0.15, duration: 0.5 }}
                    viewport={{ once: true }}
                    whileHover={{ scale: 1.05, y: -5 }}
                  >
                    {plan.destacado && <div className="hz-pricing-badge">Popular</div>}
                    <h3 className="hz-pricing-name">{plan.nombre}</h3>
                    <div className="hz-pricing-price">
                      <span className="hz-price-amount">{plan.precio}</span>
                      <span className="hz-price-period">{plan.periodo}</span>
                    </div>
                    {plan.crypto && <div className="hz-pricing-crypto">o {plan.crypto}</div>}
                    <ul className="hz-pricing-features">
                      {plan.features.map((f, j) => (
                        <li key={j}><FiCheck /> {f}</li>
                      ))}
                    </ul>
                    <button
                      className={plan.destacado ? 'hz-btn-primary' : 'hz-btn-ghost'}
                      onClick={() => navigate('/registro')}
                    >
                      {plan.botonTexto}
                    </button>
                  </motion.div>
                ))}
              </div>
            </div>
          </section>

          {/* PANEL 4: CTA + Footer */}
          <section className="hz-panel hz-panel-cta">
            <div className="hz-cta-bg">
              <Canvas
                camera={{ position: [0, 0, 5], fov: 60 }}
                style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
                gl={{ alpha: true }}
              >
                <ambientLight intensity={0.3} />
                <pointLight position={[0, 0, 3]} intensity={1.5} color="#ff6b35" />
                <FloatingOrb position={[0, 0, 0]} color="#ff6b35" speed={0.3} distort={0.7} size={2.5} />
                <Stars radius={40} depth={30} count={1000} factor={4} saturation={0.3} fade speed={0.8} />
              </Canvas>
            </div>
            <div className="hz-cta-content">
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                whileInView={{ opacity: 1, scale: 1 }}
                viewport={{ once: true }}
              >
                <h2 className="hz-cta-title">
                  Listo para empezar?
                </h2>
                <p className="hz-cta-subtitle">
                  Proba GastronomiApp gratis por 7 dias y lleva tu negocio al siguiente nivel.
                </p>
                <button className="hz-btn-primary hz-btn-xl" onClick={() => navigate('/registro')}>
                  Crear mi cuenta gratis <FiArrowRight />
                </button>
              </motion.div>
              <footer className="hz-footer">
                <span>GastronomiApp &copy; 2026</span>
                <span>admin@gastronomiapp.com</span>
              </footer>
            </div>
          </section>

        </motion.div>
      </div>
    </div>
  );
}
