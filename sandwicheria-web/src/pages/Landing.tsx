import { useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, useScroll, useTransform } from 'framer-motion';
import { Canvas, useFrame } from '@react-three/fiber';
import { Float, Environment } from '@react-three/drei';
import { FiCheck, FiArrowRight, FiShoppingCart, FiBarChart2, FiPackage, FiSmartphone } from 'react-icons/fi';
import * as THREE from 'three';

// ============================================
// 3D FOOD COMPONENTS
// ============================================

function Hamburger({ position, scale = 1, rotSpeed = 0.3 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame((state) => {
    if (group.current) {
      group.current.rotation.y = state.clock.elapsedTime * rotSpeed;
      group.current.rotation.z = Math.sin(state.clock.elapsedTime * 0.5) * 0.1;
    }
  });
  return (
    <Float speed={2} rotationIntensity={0.3} floatIntensity={1.5}>
      <group ref={group} position={position} scale={scale}>
        {/* Top bun */}
        <mesh position={[0, 0.55, 0]}>
          <sphereGeometry args={[0.75, 32, 16, 0, Math.PI * 2, 0, Math.PI / 2]} />
          <meshStandardMaterial color="#f4a460" roughness={0.6} />
        </mesh>
        {/* Sesame seeds */}
        {[[-0.3, 0.85, 0.2], [0.2, 0.9, -0.1], [0, 0.92, 0.3], [-0.15, 0.88, -0.3], [0.3, 0.82, 0.1]].map((pos, i) => (
          <mesh key={i} position={pos as [number, number, number]} scale={[0.06, 0.03, 0.06]}>
            <sphereGeometry args={[1, 8, 8]} />
            <meshStandardMaterial color="#fffdd0" />
          </mesh>
        ))}
        {/* Lettuce */}
        <mesh position={[0, 0.4, 0]}>
          <cylinderGeometry args={[0.82, 0.8, 0.12, 16]} />
          <meshStandardMaterial color="#7ec850" roughness={0.8} />
        </mesh>
        {/* Tomato */}
        <mesh position={[0, 0.28, 0]}>
          <cylinderGeometry args={[0.72, 0.72, 0.1, 16]} />
          <meshStandardMaterial color="#e74c3c" roughness={0.5} />
        </mesh>
        {/* Cheese */}
        <mesh position={[0, 0.18, 0]} rotation={[0, 0.3, 0]}>
          <boxGeometry args={[1.4, 0.06, 1.4]} />
          <meshStandardMaterial color="#ffd700" roughness={0.4} metalness={0.1} />
        </mesh>
        {/* Patty */}
        <mesh position={[0, 0.05, 0]}>
          <cylinderGeometry args={[0.7, 0.7, 0.2, 16]} />
          <meshStandardMaterial color="#6b3a2a" roughness={0.8} />
        </mesh>
        {/* Bottom bun */}
        <mesh position={[0, -0.15, 0]}>
          <cylinderGeometry args={[0.75, 0.78, 0.25, 16]} />
          <meshStandardMaterial color="#e8a952" roughness={0.6} />
        </mesh>
      </group>
    </Float>
  );
}

function Sandwich({ position, scale = 1, rotSpeed = 0.4 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame((state) => {
    if (group.current) {
      group.current.rotation.y = state.clock.elapsedTime * rotSpeed;
    }
  });
  return (
    <Float speed={1.8} rotationIntensity={0.4} floatIntensity={1.2}>
      <group ref={group} position={position} scale={scale} rotation={[0.2, 0, 0.1]}>
        {/* Top bread slice */}
        <mesh position={[0, 0.35, 0]} rotation={[0, 0, 0.05]}>
          <boxGeometry args={[1.6, 0.2, 0.9]} />
          <meshStandardMaterial color="#e8a952" roughness={0.7} />
        </mesh>
        {/* Ham */}
        <mesh position={[0, 0.2, 0]}>
          <boxGeometry args={[1.5, 0.08, 0.85]} />
          <meshStandardMaterial color="#f5a0b5" roughness={0.5} />
        </mesh>
        {/* Cheese */}
        <mesh position={[0.05, 0.1, 0]}>
          <boxGeometry args={[1.55, 0.06, 0.88]} />
          <meshStandardMaterial color="#ffd700" roughness={0.4} />
        </mesh>
        {/* Lettuce */}
        <mesh position={[-0.05, 0.0, 0]}>
          <boxGeometry args={[1.65, 0.08, 0.92]} />
          <meshStandardMaterial color="#7ec850" roughness={0.8} />
        </mesh>
        {/* Tomato slices */}
        <mesh position={[0, -0.08, 0]}>
          <boxGeometry args={[1.5, 0.06, 0.85]} />
          <meshStandardMaterial color="#e74c3c" roughness={0.5} />
        </mesh>
        {/* Bottom bread */}
        <mesh position={[0, -0.25, 0]}>
          <boxGeometry args={[1.6, 0.2, 0.9]} />
          <meshStandardMaterial color="#daa44c" roughness={0.7} />
        </mesh>
      </group>
    </Float>
  );
}

function SodaCup({ position, scale = 1, color = '#ff69b4' }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame((state) => {
    if (group.current) {
      group.current.rotation.y = state.clock.elapsedTime * 0.5;
      group.current.position.y = position[1] + Math.sin(state.clock.elapsedTime * 0.8) * 0.15;
    }
  });
  return (
    <Float speed={2.5} rotationIntensity={0.2} floatIntensity={1}>
      <group ref={group} position={position} scale={scale}>
        {/* Cup body */}
        <mesh position={[0, 0, 0]}>
          <cylinderGeometry args={[0.4, 0.32, 1.2, 16]} />
          <meshStandardMaterial color={color} roughness={0.3} metalness={0.1} />
        </mesh>
        {/* Cup stripe */}
        <mesh position={[0, 0.1, 0]}>
          <cylinderGeometry args={[0.42, 0.38, 0.15, 16]} />
          <meshStandardMaterial color="#ffffff" roughness={0.3} />
        </mesh>
        {/* Lid */}
        <mesh position={[0, 0.65, 0]}>
          <cylinderGeometry args={[0.44, 0.42, 0.1, 16]} />
          <meshStandardMaterial color="#ffffff" roughness={0.2} metalness={0.3} />
        </mesh>
        {/* Straw */}
        <mesh position={[0.1, 1.1, 0]} rotation={[0, 0, 0.15]}>
          <cylinderGeometry args={[0.03, 0.03, 1, 8]} />
          <meshStandardMaterial color="#ff1493" roughness={0.3} />
        </mesh>
      </group>
    </Float>
  );
}

function Milkshake({ position, scale = 1 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame((state) => {
    if (group.current) {
      group.current.rotation.y = state.clock.elapsedTime * 0.35;
    }
  });
  return (
    <Float speed={1.5} rotationIntensity={0.3} floatIntensity={1.8}>
      <group ref={group} position={position} scale={scale}>
        {/* Glass */}
        <mesh position={[0, 0, 0]}>
          <cylinderGeometry args={[0.45, 0.3, 1.4, 16]} />
          <meshStandardMaterial color="#87ceeb" transparent opacity={0.4} roughness={0.1} metalness={0.2} />
        </mesh>
        {/* Milkshake liquid */}
        <mesh position={[0, -0.05, 0]}>
          <cylinderGeometry args={[0.42, 0.28, 1.2, 16]} />
          <meshStandardMaterial color="#ffb6c1" roughness={0.4} />
        </mesh>
        {/* Whipped cream */}
        <mesh position={[0, 0.85, 0]}>
          <sphereGeometry args={[0.4, 16, 16]} />
          <meshStandardMaterial color="#fff5ee" roughness={0.8} />
        </mesh>
        {/* Cherry */}
        <mesh position={[0, 1.15, 0]}>
          <sphereGeometry args={[0.12, 12, 12]} />
          <meshStandardMaterial color="#dc143c" roughness={0.3} metalness={0.2} />
        </mesh>
        {/* Cherry stem */}
        <mesh position={[0.02, 1.3, 0]} rotation={[0, 0, 0.2]}>
          <cylinderGeometry args={[0.015, 0.015, 0.2, 6]} />
          <meshStandardMaterial color="#228b22" />
        </mesh>
        {/* Straw */}
        <mesh position={[0.15, 0.9, 0.1]} rotation={[0.1, 0, 0.2]}>
          <cylinderGeometry args={[0.035, 0.035, 1.4, 8]} />
          <meshStandardMaterial color="#ff69b4" />
        </mesh>
      </group>
    </Float>
  );
}

function FrenchFries({ position, scale = 1 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame((state) => {
    if (group.current) {
      group.current.rotation.y = state.clock.elapsedTime * 0.4;
    }
  });
  return (
    <Float speed={2} rotationIntensity={0.5} floatIntensity={1.2}>
      <group ref={group} position={position} scale={scale}>
        {/* Container */}
        <mesh position={[0, -0.1, 0]}>
          <boxGeometry args={[0.7, 0.8, 0.5]} />
          <meshStandardMaterial color="#e74c3c" roughness={0.5} />
        </mesh>
        {/* Fries */}
        {[
          [-0.15, 0.5, 0, 0.15], [0.1, 0.55, 0.05, -0.1], [0, 0.6, -0.08, 0.08],
          [-0.2, 0.45, 0.1, -0.12], [0.18, 0.5, -0.05, 0.18], [0.05, 0.52, 0.12, -0.05],
        ].map(([x, y, z, rot], i) => (
          <mesh key={i} position={[x, y, z] as [number, number, number]} rotation={[rot * 0.5, 0, rot]}>
            <boxGeometry args={[0.08, 0.7, 0.08]} />
            <meshStandardMaterial color="#ffd700" roughness={0.6} />
          </mesh>
        ))}
      </group>
    </Float>
  );
}

function Donut({ position, scale = 1, color = '#ff69b4' }: any) {
  const meshRef = useRef<THREE.Mesh>(null!);
  useFrame((state) => {
    if (meshRef.current) {
      meshRef.current.rotation.x = state.clock.elapsedTime * 0.5;
      meshRef.current.rotation.y = state.clock.elapsedTime * 0.3;
    }
  });
  return (
    <Float speed={3} rotationIntensity={0.6} floatIntensity={2}>
      <group position={position} scale={scale}>
        {/* Donut base */}
        <mesh ref={meshRef}>
          <torusGeometry args={[0.5, 0.25, 16, 32]} />
          <meshStandardMaterial color="#daa44c" roughness={0.6} />
        </mesh>
        {/* Glaze */}
        <mesh rotation={[meshRef.current?.rotation.x || 0, meshRef.current?.rotation.y || 0, 0]}>
          <torusGeometry args={[0.5, 0.26, 16, 32, Math.PI]} />
          <meshStandardMaterial color={color} roughness={0.3} metalness={0.1} />
        </mesh>
      </group>
    </Float>
  );
}

function BubbleParticles() {
  const count = 30;
  const meshRef = useRef<THREE.InstancedMesh>(null!);
  const dummy = new THREE.Object3D();

  useFrame((state) => {
    if (!meshRef.current) return;
    for (let i = 0; i < count; i++) {
      const t = state.clock.elapsedTime;
      const x = Math.sin(i * 1.3 + t * 0.2) * 8;
      const y = ((i * 0.7 + t * 0.3) % 10) - 5;
      const z = Math.cos(i * 0.9 + t * 0.15) * 6 - 4;
      const s = 0.05 + Math.sin(i + t) * 0.03;
      dummy.position.set(x, y, z);
      dummy.scale.set(s, s, s);
      dummy.updateMatrix();
      meshRef.current.setMatrixAt(i, dummy.matrix);
    }
    meshRef.current.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh ref={meshRef} args={[undefined, undefined, count]}>
      <sphereGeometry args={[1, 8, 8]} />
      <meshStandardMaterial color="#ffb6c1" transparent opacity={0.4} />
    </instancedMesh>
  );
}

// ============================================
// SCENES
// ============================================

function HeroScene() {
  return (
    <Canvas
      camera={{ position: [0, 1, 8], fov: 55 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true, antialias: true }}
    >
      <ambientLight intensity={0.6} />
      <directionalLight position={[5, 5, 5]} intensity={1} color="#fff5f5" />
      <pointLight position={[-4, 3, 3]} intensity={0.8} color="#ff69b4" />
      <pointLight position={[4, -2, 2]} intensity={0.5} color="#87ceeb" />

      <Hamburger position={[3.5, 0.5, -1]} scale={1.3} rotSpeed={0.3} />
      <Milkshake position={[-3.5, -0.5, -2]} scale={0.9} />
      <FrenchFries position={[5, -1.5, -3]} scale={0.8} />
      <Donut position={[-1.5, 2.5, -3]} scale={0.7} color="#ff69b4" />
      <SodaCup position={[1, -2, -2]} scale={0.6} color="#87ceeb" />
      <BubbleParticles />

      <Environment preset="apartment" />
    </Canvas>
  );
}

function FeaturesScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 7], fov: 50 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.5} />
      <pointLight position={[3, 3, 3]} intensity={0.8} color="#ff69b4" />
      <pointLight position={[-3, -2, 2]} intensity={0.5} color="#87ceeb" />

      <Sandwich position={[3.5, -1.5, -2]} scale={0.8} />
      <Donut position={[-3, 2, -3]} scale={0.6} color="#87ceeb" />
      <SodaCup position={[-4, -2, -2]} scale={0.5} color="#ff69b4" />
      <BubbleParticles />

      <Environment preset="apartment" />
    </Canvas>
  );
}

function PricingScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 8], fov: 50 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.5} />
      <pointLight position={[-3, 4, 3]} intensity={1} color="#ffb6c1" />
      <pointLight position={[3, -2, 2]} intensity={0.5} color="#87ceeb" />

      <Hamburger position={[-4.5, -2, -3]} scale={0.9} rotSpeed={0.2} />
      <Milkshake position={[4, 1.5, -2]} scale={0.7} />
      <FrenchFries position={[-3, 2.5, -4]} scale={0.6} />
      <BubbleParticles />

      <Environment preset="apartment" />
    </Canvas>
  );
}

function CtaScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 6], fov: 55 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.4} />
      <pointLight position={[0, 2, 3]} intensity={1.5} color="#ff69b4" />
      <pointLight position={[-2, -1, 2]} intensity={0.6} color="#87ceeb" />

      <Hamburger position={[0, 0, 0]} scale={2} rotSpeed={0.15} />
      <Donut position={[-2.5, 1.5, -2]} scale={0.5} color="#ffb6c1" />
      <Donut position={[2.5, -1.5, -2]} scale={0.5} color="#87ceeb" />
      <SodaCup position={[-3, -2, -1]} scale={0.5} color="#ff69b4" />
      <SodaCup position={[3, 2, -1]} scale={0.5} color="#87ceeb" />
      <BubbleParticles />

      <Environment preset="apartment" />
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
// MAIN LANDING
// ============================================

export default function Landing() {
  const navigate = useNavigate();
  const containerRef = useRef<HTMLDivElement>(null);

  const { scrollYProgress } = useScroll({ target: containerRef });
  const x = useTransform(scrollYProgress, [0, 1], ['0%', '-75%']);

  return (
    <div className="hz-landing" ref={containerRef}>
      {/* Navbar */}
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

      {/* Horizontal panels */}
      <div className="hz-sticky-wrap">
        <motion.div className="hz-panels" style={{ x }}>

          {/* PANEL 1: Hero */}
          <section className="hz-panel hz-panel-hero">
            <HeroScene />
            <div className="hz-hero-content">
              <motion.div
                initial={{ opacity: 0, x: -60 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.8, ease: 'easeOut' }}
              >
                <div className="hz-hero-tag">Sistema de Gestion Gastronomica</div>
                <h1 className="hz-hero-title">
                  Tu negocio<br />gastronomico,<br />
                  <span className="hz-text-accent">bajo control total.</span>
                </h1>
                <p className="hz-hero-subtitle">
                  Ventas, stock, reportes y facturacion.<br />
                  Todo en un solo lugar, simple y rapido.
                </p>
                <div className="hz-hero-actions">
                  <button className="hz-btn-primary hz-btn-lg" onClick={() => navigate('/registro')}>
                    Empezar gratis <FiArrowRight />
                  </button>
                  <button className="hz-btn-ghost" onClick={() => navigate('/login')}>
                    Ya tengo cuenta
                  </button>
                </div>
                <p className="hz-hero-note">7 dias gratis. Sin tarjeta de credito.</p>
              </motion.div>
            </div>
            <div className="hz-scroll-indicator">
              <motion.div
                animate={{ x: [0, 12, 0] }}
                transition={{ repeat: Infinity, duration: 1.5, ease: 'easeInOut' }}
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
                    transition={{ delay: i * 0.12, duration: 0.5 }}
                    viewport={{ once: true }}
                    whileHover={{ scale: 1.03, y: -4 }}
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
                Planes simples, sin sorpresas
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
                    whileHover={{ scale: 1.04, y: -6 }}
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

          {/* PANEL 4: CTA */}
          <section className="hz-panel hz-panel-cta">
            <CtaScene />
            <div className="hz-cta-content">
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                whileInView={{ opacity: 1, scale: 1 }}
                viewport={{ once: true }}
              >
                <h2 className="hz-cta-title">Listo para empezar?</h2>
                <p className="hz-cta-subtitle">
                  Proba GastronomiApp gratis por 7 dias y<br />lleva tu negocio al siguiente nivel.
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
