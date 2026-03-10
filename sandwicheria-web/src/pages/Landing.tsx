import { useRef, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, useScroll, useTransform } from 'framer-motion';
import { Canvas, useFrame } from '@react-three/fiber';
import { Float, Environment, Sparkles, ContactShadows, RoundedBox } from '@react-three/drei';
import { FiCheck, FiArrowRight, FiShoppingCart, FiBarChart2, FiPackage, FiSmartphone } from 'react-icons/fi';
import * as THREE from 'three';

// ============================================
// 3D FOOD — PIZZA
// ============================================

function Pizza({ position, scale = 1, rotSpeed = 0.25 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.y = clock.elapsedTime * rotSpeed;
      group.current.rotation.x = Math.sin(clock.elapsedTime * 0.3) * 0.08;
    }
  });
  // Pepperoni positions on the pizza surface
  const pepperoniPositions = useMemo(() => [
    [0.3, 0.12, 0.15], [-0.25, 0.12, 0.35], [0.1, 0.12, -0.3],
    [-0.4, 0.12, -0.1], [0.45, 0.12, -0.15], [-0.1, 0.12, 0.5],
    [0.35, 0.12, 0.4], [-0.35, 0.12, -0.35],
  ], []);
  // Olive positions
  const olivePositions = useMemo(() => [
    [0.15, 0.13, 0.4], [-0.45, 0.13, 0.2], [0.4, 0.13, 0.05],
    [-0.15, 0.13, -0.4], [0.25, 0.13, -0.35],
  ], []);

  return (
    <Float speed={1.5} rotationIntensity={0.2} floatIntensity={1.2}>
      <group ref={group} position={position} scale={scale}>
        {/* Pizza base/dough — flat cylinder */}
        <mesh position={[0, 0, 0]}>
          <cylinderGeometry args={[0.85, 0.88, 0.1, 32]} />
          <meshStandardMaterial color="#e8c06a" roughness={0.75} />
        </mesh>
        {/* Crust ring — torus around the edge */}
        <mesh position={[0, 0.04, 0]} rotation={[Math.PI / 2, 0, 0]}>
          <torusGeometry args={[0.82, 0.08, 8, 32]} />
          <meshStandardMaterial color="#d4a043" roughness={0.8} />
        </mesh>
        {/* Sauce layer */}
        <mesh position={[0, 0.06, 0]}>
          <cylinderGeometry args={[0.72, 0.72, 0.03, 32]} />
          <meshStandardMaterial color="#c0392b" roughness={0.9} />
        </mesh>
        {/* Cheese layer — slightly bumpy */}
        <mesh position={[0, 0.09, 0]}>
          <cylinderGeometry args={[0.73, 0.7, 0.04, 24]} />
          <meshStandardMaterial color="#f5d77b" roughness={0.6} emissive="#f5d77b" emissiveIntensity={0.05} />
        </mesh>
        {/* Pepperoni slices */}
        {pepperoniPositions.map((pos, i) => (
          <mesh key={`pep-${i}`} position={pos as [number, number, number]}>
            <cylinderGeometry args={[0.08, 0.08, 0.03, 12]} />
            <meshStandardMaterial color="#8b2500" roughness={0.7} />
          </mesh>
        ))}
        {/* Olives */}
        {olivePositions.map((pos, i) => (
          <mesh key={`olv-${i}`} position={pos as [number, number, number]}>
            <torusGeometry args={[0.04, 0.02, 8, 12]} />
            <meshStandardMaterial color="#2d2d2d" roughness={0.5} />
          </mesh>
        ))}
        {/* Basil leaves — small green flat boxes */}
        {[[0.0, 0.14, 0.1], [-0.3, 0.14, 0.0], [0.3, 0.14, -0.2]].map((pos, i) => (
          <mesh key={`basil-${i}`} position={pos as [number, number, number]} rotation={[0, i * 1.2, 0]}>
            <boxGeometry args={[0.1, 0.01, 0.06]} />
            <meshStandardMaterial color="#27ae60" roughness={0.8} />
          </mesh>
        ))}
      </group>
    </Float>
  );
}

// ============================================
// 3D FOOD — HAMBURGER (detailed)
// ============================================

function Hamburger({ position, scale = 1, rotSpeed = 0.3 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.y = clock.elapsedTime * rotSpeed;
      group.current.rotation.z = Math.sin(clock.elapsedTime * 0.4) * 0.06;
    }
  });
  return (
    <Float speed={2} rotationIntensity={0.3} floatIntensity={1.5}>
      <group ref={group} position={position} scale={scale}>
        {/* Top bun — dome */}
        <mesh position={[0, 0.6, 0]}>
          <sphereGeometry args={[0.72, 32, 16, 0, Math.PI * 2, 0, Math.PI / 2]} />
          <meshStandardMaterial color="#e8a44a" roughness={0.55} />
        </mesh>
        {/* Sesame seeds */}
        {[[-0.3, 0.88, 0.2], [0.2, 0.92, -0.15], [0, 0.95, 0.25], [-0.15, 0.9, -0.3], [0.28, 0.85, 0.08], [0.1, 0.93, -0.05], [-0.25, 0.87, -0.1]].map((pos, i) => (
          <mesh key={i} position={pos as [number, number, number]} rotation={[Math.random() * 0.5, 0, Math.random() * 0.5]}>
            <sphereGeometry args={[0.03, 6, 6]} />
            <meshStandardMaterial color="#fffdd0" emissive="#fffdd0" emissiveIntensity={0.1} roughness={0.4} />
          </mesh>
        ))}
        {/* Top bun bottom flat */}
        <mesh position={[0, 0.55, 0]}>
          <cylinderGeometry args={[0.72, 0.72, 0.08, 24]} />
          <meshStandardMaterial color="#d4943c" roughness={0.6} />
        </mesh>
        {/* Lettuce — wavy ring */}
        <mesh position={[0, 0.42, 0]}>
          <cylinderGeometry args={[0.82, 0.78, 0.1, 20]} />
          <meshStandardMaterial color="#6abf47" roughness={0.85} />
        </mesh>
        {/* Tomato slices — 2 layers */}
        <mesh position={[0, 0.33, 0]}>
          <cylinderGeometry args={[0.68, 0.68, 0.06, 16]} />
          <meshStandardMaterial color="#e74c3c" roughness={0.5} emissive="#e74c3c" emissiveIntensity={0.03} />
        </mesh>
        {/* Cheese — melted look, overhanging */}
        <mesh position={[0, 0.24, 0]} rotation={[0, 0.4, 0]}>
          <boxGeometry args={[1.35, 0.05, 1.35]} />
          <meshStandardMaterial color="#ffc425" roughness={0.35} emissive="#ffc425" emissiveIntensity={0.08} />
        </mesh>
        {/* Patty */}
        <mesh position={[0, 0.12, 0]}>
          <cylinderGeometry args={[0.68, 0.65, 0.18, 20]} />
          <meshStandardMaterial color="#5a2d0c" roughness={0.85} />
        </mesh>
        {/* Patty grill marks */}
        {[-0.2, 0, 0.2].map((z, i) => (
          <mesh key={`grill-${i}`} position={[0, 0.22, z]}>
            <boxGeometry args={[1.0, 0.01, 0.03]} />
            <meshStandardMaterial color="#3d1a00" roughness={0.9} />
          </mesh>
        ))}
        {/* Onion ring */}
        <mesh position={[0, 0.02, 0]} rotation={[Math.PI / 2, 0, 0]}>
          <torusGeometry args={[0.3, 0.03, 8, 20]} />
          <meshStandardMaterial color="#f0e6d6" roughness={0.5} transparent opacity={0.85} />
        </mesh>
        {/* Bottom bun */}
        <mesh position={[0, -0.1, 0]}>
          <cylinderGeometry args={[0.72, 0.76, 0.22, 24]} />
          <meshStandardMaterial color="#d4943c" roughness={0.6} />
        </mesh>
      </group>
    </Float>
  );
}

// ============================================
// 3D FOOD — SANDWICH (club style)
// ============================================

function Sandwich({ position, scale = 1, rotSpeed = 0.35 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.y = clock.elapsedTime * rotSpeed;
    }
  });
  return (
    <Float speed={1.8} rotationIntensity={0.4} floatIntensity={1.3}>
      <group ref={group} position={position} scale={scale} rotation={[0.15, 0, 0.08]}>
        {/* Top toast — rounded */}
        <RoundedBox args={[1.5, 0.18, 0.85]} radius={0.04} position={[0, 0.5, 0]}>
          <meshStandardMaterial color="#d4943c" roughness={0.65} />
        </RoundedBox>
        {/* Toothpick */}
        <mesh position={[0, 0.8, 0]}>
          <cylinderGeometry args={[0.015, 0.015, 0.7, 6]} />
          <meshStandardMaterial color="#c4a672" roughness={0.5} />
        </mesh>
        {/* Olive on toothpick */}
        <mesh position={[0, 1.15, 0]}>
          <sphereGeometry args={[0.06, 10, 10]} />
          <meshStandardMaterial color="#2d5a1e" roughness={0.6} />
        </mesh>
        {/* Turkey/ham */}
        <RoundedBox args={[1.45, 0.07, 0.82]} radius={0.02} position={[0, 0.37, 0]}>
          <meshStandardMaterial color="#f0a0b5" roughness={0.5} />
        </RoundedBox>
        {/* Swiss cheese */}
        <RoundedBox args={[1.42, 0.05, 0.8]} radius={0.02} position={[0, 0.3, 0]}>
          <meshStandardMaterial color="#ffd700" roughness={0.4} emissive="#ffd700" emissiveIntensity={0.04} />
        </RoundedBox>
        {/* Lettuce */}
        <RoundedBox args={[1.55, 0.06, 0.9]} radius={0.02} position={[0, 0.24, 0]}>
          <meshStandardMaterial color="#6abf47" roughness={0.85} />
        </RoundedBox>
        {/* Middle bread */}
        <RoundedBox args={[1.5, 0.15, 0.85]} radius={0.04} position={[0, 0.14, 0]}>
          <meshStandardMaterial color="#d4943c" roughness={0.65} />
        </RoundedBox>
        {/* Tomato */}
        <RoundedBox args={[1.42, 0.05, 0.8]} radius={0.02} position={[0, 0.04, 0]}>
          <meshStandardMaterial color="#e74c3c" roughness={0.5} />
        </RoundedBox>
        {/* Bacon */}
        {[-0.15, 0.15].map((x, i) => (
          <mesh key={`bacon-${i}`} position={[x, -0.03, 0]} rotation={[0, i * 0.3, 0]}>
            <boxGeometry args={[1.3, 0.04, 0.18]} />
            <meshStandardMaterial color="#8b3a3a" roughness={0.7} />
          </mesh>
        ))}
        {/* Bottom toast */}
        <RoundedBox args={[1.5, 0.18, 0.85]} radius={0.04} position={[0, -0.16, 0]}>
          <meshStandardMaterial color="#c4873a" roughness={0.65} />
        </RoundedBox>
      </group>
    </Float>
  );
}

// ============================================
// 3D FOOD — MILKSHAKE (glass effect)
// ============================================

function Milkshake({ position, scale = 1 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.y = clock.elapsedTime * 0.3;
    }
  });
  return (
    <Float speed={1.5} rotationIntensity={0.3} floatIntensity={1.8}>
      <group ref={group} position={position} scale={scale}>
        {/* Glass — transparent */}
        <mesh position={[0, 0, 0]}>
          <cylinderGeometry args={[0.42, 0.28, 1.5, 20]} />
          <meshStandardMaterial color="#b0e0e6" transparent opacity={0.3} roughness={0.05} metalness={0.15} />
        </mesh>
        {/* Milkshake liquid — strawberry pink */}
        <mesh position={[0, -0.08, 0]}>
          <cylinderGeometry args={[0.39, 0.26, 1.25, 20]} />
          <meshStandardMaterial color="#ff69b4" roughness={0.4} emissive="#ff69b4" emissiveIntensity={0.06} />
        </mesh>
        {/* Whipped cream — stacked spheres */}
        <mesh position={[0, 0.85, 0]}>
          <sphereGeometry args={[0.38, 16, 16]} />
          <meshStandardMaterial color="#fff5ee" roughness={0.9} />
        </mesh>
        <mesh position={[0.12, 0.98, 0.08]}>
          <sphereGeometry args={[0.2, 12, 12]} />
          <meshStandardMaterial color="#fff8f0" roughness={0.9} />
        </mesh>
        <mesh position={[-0.1, 1.0, -0.05]}>
          <sphereGeometry args={[0.15, 12, 12]} />
          <meshStandardMaterial color="#fffaf5" roughness={0.9} />
        </mesh>
        {/* Cherry */}
        <mesh position={[0.05, 1.2, 0]}>
          <sphereGeometry args={[0.1, 12, 12]} />
          <meshStandardMaterial color="#dc143c" roughness={0.3} emissive="#dc143c" emissiveIntensity={0.1} />
        </mesh>
        {/* Cherry stem */}
        <mesh position={[0.07, 1.38, 0]} rotation={[0, 0, 0.15]}>
          <cylinderGeometry args={[0.012, 0.012, 0.22, 6]} />
          <meshStandardMaterial color="#228b22" />
        </mesh>
        {/* Straw */}
        <mesh position={[0.18, 0.7, 0.12]} rotation={[0.12, 0, 0.18]}>
          <cylinderGeometry args={[0.032, 0.032, 1.8, 8]} />
          <meshStandardMaterial color="#ff1493" roughness={0.3} />
        </mesh>
        {/* Straw stripe */}
        <mesh position={[0.22, 1.1, 0.15]} rotation={[0.12, 0, 0.18]}>
          <cylinderGeometry args={[0.035, 0.035, 0.15, 8]} />
          <meshStandardMaterial color="#fff" roughness={0.3} />
        </mesh>
        {/* Drizzle on glass */}
        <mesh position={[0.35, 0.3, 0]} rotation={[0, 0, 0.3]}>
          <cylinderGeometry args={[0.02, 0.01, 0.4, 6]} />
          <meshStandardMaterial color="#c0392b" roughness={0.6} />
        </mesh>
      </group>
    </Float>
  );
}

// ============================================
// 3D FOOD — FRENCH FRIES
// ============================================

function FrenchFries({ position, scale = 1 }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.y = clock.elapsedTime * 0.4;
    }
  });
  const fries = useMemo(() => [
    { pos: [-0.12, 0.55, 0.05], rot: 0.12 }, { pos: [0.1, 0.6, 0.08], rot: -0.08 },
    { pos: [0, 0.65, -0.06], rot: 0.05 }, { pos: [-0.18, 0.5, 0.12], rot: -0.15 },
    { pos: [0.15, 0.58, -0.1], rot: 0.18 }, { pos: [0.05, 0.55, 0.15], rot: -0.1 },
    { pos: [-0.08, 0.62, -0.08], rot: 0.08 }, { pos: [0.2, 0.52, 0.02], rot: -0.2 },
  ], []);

  return (
    <Float speed={2} rotationIntensity={0.5} floatIntensity={1.2}>
      <group ref={group} position={position} scale={scale}>
        {/* Red container */}
        <RoundedBox args={[0.65, 0.7, 0.45]} radius={0.04} position={[0, 0, 0]}>
          <meshStandardMaterial color="#e74c3c" roughness={0.5} />
        </RoundedBox>
        {/* White stripe on container */}
        <RoundedBox args={[0.66, 0.12, 0.46]} radius={0.02} position={[0, 0.15, 0]}>
          <meshStandardMaterial color="#fff" roughness={0.4} />
        </RoundedBox>
        {/* Fries sticking out */}
        {fries.map((f, i) => (
          <mesh key={i} position={f.pos as [number, number, number]} rotation={[f.rot * 0.5, 0, f.rot]}>
            <boxGeometry args={[0.065, 0.65, 0.065]} />
            <meshStandardMaterial color="#ffd54f" roughness={0.55} emissive="#ffd54f" emissiveIntensity={0.03} />
          </mesh>
        ))}
        {/* Ketchup drip */}
        <mesh position={[0.05, 0.35, 0.22]}>
          <sphereGeometry args={[0.04, 8, 8]} />
          <meshStandardMaterial color="#c0392b" roughness={0.6} />
        </mesh>
      </group>
    </Float>
  );
}

// ============================================
// 3D FOOD — SODA CUP
// ============================================

function SodaCup({ position, scale = 1, color = '#ff69b4' }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.y = clock.elapsedTime * 0.45;
      group.current.position.y = position[1] + Math.sin(clock.elapsedTime * 0.7) * 0.1;
    }
  });
  return (
    <Float speed={2.5} rotationIntensity={0.2} floatIntensity={1}>
      <group ref={group} position={position} scale={scale}>
        {/* Cup body */}
        <mesh>
          <cylinderGeometry args={[0.38, 0.3, 1.15, 20]} />
          <meshStandardMaterial color={color} roughness={0.3} />
        </mesh>
        {/* Cup logo band */}
        <mesh position={[0, 0.05, 0]}>
          <cylinderGeometry args={[0.4, 0.36, 0.2, 20]} />
          <meshStandardMaterial color="#fff" roughness={0.3} />
        </mesh>
        {/* Lid */}
        <mesh position={[0, 0.62, 0]}>
          <cylinderGeometry args={[0.42, 0.4, 0.08, 20]} />
          <meshStandardMaterial color="#fff" roughness={0.15} metalness={0.2} />
        </mesh>
        {/* Lid dome */}
        <mesh position={[0, 0.7, 0]}>
          <sphereGeometry args={[0.38, 16, 8, 0, Math.PI * 2, 0, Math.PI / 3]} />
          <meshStandardMaterial color="#fff" transparent opacity={0.6} roughness={0.1} />
        </mesh>
        {/* Straw */}
        <mesh position={[0.08, 1.05, 0]} rotation={[0, 0, 0.12]}>
          <cylinderGeometry args={[0.028, 0.028, 1.0, 8]} />
          <meshStandardMaterial color="#ff1493" roughness={0.3} />
        </mesh>
        {/* Ice condensation drops */}
        {[[0.33, 0.1, 0.15], [-0.28, -0.2, 0.22], [0.2, -0.3, -0.25]].map((pos, i) => (
          <mesh key={i} position={pos as [number, number, number]}>
            <sphereGeometry args={[0.025, 6, 6]} />
            <meshStandardMaterial color="#87ceeb" transparent opacity={0.5} roughness={0.05} />
          </mesh>
        ))}
      </group>
    </Float>
  );
}

// ============================================
// 3D FOOD — DONUT
// ============================================

function Donut({ position, scale = 1, glaze = '#ff69b4' }: any) {
  const group = useRef<THREE.Group>(null!);
  useFrame(({ clock }) => {
    if (group.current) {
      group.current.rotation.x = clock.elapsedTime * 0.5;
      group.current.rotation.y = clock.elapsedTime * 0.3;
    }
  });
  const sprinkleColors = ['#ff1493', '#ffd700', '#87ceeb', '#7ec850', '#ff6347', '#fff'];
  const sprinkles = useMemo(() =>
    Array.from({ length: 18 }, (_, i) => {
      const angle = (i / 18) * Math.PI * 2;
      const r = 0.5 + Math.random() * 0.05;
      return {
        pos: [Math.cos(angle) * r, 0.28 + Math.random() * 0.02, Math.sin(angle) * r],
        rot: [Math.random(), Math.random(), Math.random()],
        color: sprinkleColors[i % sprinkleColors.length],
      };
    }), []);

  return (
    <Float speed={3} rotationIntensity={0.6} floatIntensity={2}>
      <group ref={group} position={position} scale={scale}>
        {/* Donut body */}
        <mesh>
          <torusGeometry args={[0.5, 0.25, 20, 36]} />
          <meshStandardMaterial color="#d4943c" roughness={0.65} />
        </mesh>
        {/* Glaze — top half */}
        <mesh position={[0, 0.02, 0]}>
          <torusGeometry args={[0.5, 0.26, 20, 36, Math.PI * 2]} />
          <meshStandardMaterial color={glaze} roughness={0.25} emissive={glaze} emissiveIntensity={0.08} />
        </mesh>
        {/* Sprinkles */}
        {sprinkles.map((s, i) => (
          <mesh key={i} position={s.pos as [number, number, number]} rotation={s.rot as [number, number, number]}>
            <boxGeometry args={[0.04, 0.015, 0.012]} />
            <meshStandardMaterial color={s.color} roughness={0.4} />
          </mesh>
        ))}
      </group>
    </Float>
  );
}

// ============================================
// BUBBLE PARTICLES
// ============================================

function FoodSparkles() {
  return (
    <Sparkles
      count={50}
      speed={0.4}
      size={2}
      color="#ffb6c1"
      scale={[15, 10, 8]}
      opacity={0.4}
    />
  );
}

// ============================================
// SCENES
// ============================================

function HeroScene() {
  return (
    <Canvas
      camera={{ position: [0, 1.5, 9], fov: 52 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true, antialias: true }}
    >
      <ambientLight intensity={0.55} />
      <directionalLight position={[5, 8, 5]} intensity={1.1} color="#fff5f5" castShadow />
      <pointLight position={[-4, 3, 4]} intensity={0.7} color="#ff69b4" />
      <pointLight position={[4, -2, 3]} intensity={0.5} color="#87ceeb" />
      <spotLight position={[0, 6, 2]} angle={0.4} penumbra={0.5} intensity={0.6} color="#ffb6c1" />

      <Hamburger position={[3.8, 0.8, -0.5]} scale={1.4} rotSpeed={0.25} />
      <Pizza position={[-3, -0.5, -2]} scale={1.1} rotSpeed={0.2} />
      <Milkshake position={[-4.5, 1, -1]} scale={0.75} />
      <FrenchFries position={[5.5, -1.5, -2.5]} scale={0.85} />
      <Donut position={[-1.5, 2.8, -3]} scale={0.65} glaze="#87ceeb" />
      <SodaCup position={[1.5, -2.2, -1.5]} scale={0.55} color="#ff69b4" />

      <FoodSparkles />
      <ContactShadows position={[0, -3, 0]} opacity={0.3} scale={20} blur={2} far={5} />
      <Environment preset="apartment" />
    </Canvas>
  );
}

function FeaturesScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 8], fov: 48 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.5} />
      <pointLight position={[4, 3, 3]} intensity={0.8} color="#ff69b4" />
      <pointLight position={[-3, -2, 3]} intensity={0.5} color="#87ceeb" />

      <Sandwich position={[4, -1.2, -1.5]} scale={0.85} />
      <Pizza position={[-4, 1.5, -3]} scale={0.7} rotSpeed={0.15} />
      <Donut position={[3, 2.5, -4]} scale={0.5} glaze="#ff69b4" />
      <SodaCup position={[-5, -1.8, -2]} scale={0.5} color="#87ceeb" />

      <FoodSparkles />
      <Environment preset="apartment" />
    </Canvas>
  );
}

function PricingScene() {
  return (
    <Canvas
      camera={{ position: [0, 0, 9], fov: 48 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.45} />
      <pointLight position={[-4, 4, 3]} intensity={1} color="#ffb6c1" />
      <pointLight position={[4, -2, 3]} intensity={0.5} color="#87ceeb" />

      <Hamburger position={[-5, -2, -2.5]} scale={0.9} rotSpeed={0.18} />
      <Milkshake position={[5, 1.5, -2]} scale={0.7} />
      <FrenchFries position={[-3.5, 2.5, -3.5]} scale={0.6} />
      <Donut position={[4, -2, -3]} scale={0.5} glaze="#ffd700" />

      <FoodSparkles />
      <Environment preset="apartment" />
    </Canvas>
  );
}

function CtaScene() {
  return (
    <Canvas
      camera={{ position: [0, 1, 7], fov: 52 }}
      style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      gl={{ alpha: true }}
    >
      <ambientLight intensity={0.4} />
      <pointLight position={[0, 3, 4]} intensity={1.5} color="#ff69b4" />
      <pointLight position={[-3, -1, 3]} intensity={0.6} color="#87ceeb" />
      <spotLight position={[0, 5, 3]} angle={0.5} penumbra={0.6} intensity={0.8} color="#ffb6c1" />

      <Pizza position={[0, 0.5, 0]} scale={2} rotSpeed={0.12} />
      <Hamburger position={[-3, -1, -2]} scale={0.7} rotSpeed={0.2} />
      <Sandwich position={[3.5, 1, -2.5]} scale={0.55} />
      <Donut position={[-2, 2.5, -3]} scale={0.45} glaze="#ff69b4" />
      <Donut position={[2.5, -2, -3]} scale={0.4} glaze="#87ceeb" />
      <Milkshake position={[4.5, -1.5, -1.5]} scale={0.5} />
      <FrenchFries position={[-4, -2.5, -2]} scale={0.5} />

      <FoodSparkles />
      <ContactShadows position={[0, -3.5, 0]} opacity={0.25} scale={18} blur={2.5} />
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
              <motion.h2 className="hz-section-title" initial={{ opacity: 0 }} whileInView={{ opacity: 1 }} viewport={{ once: true }}>
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
              <motion.h2 className="hz-section-title" initial={{ opacity: 0 }} whileInView={{ opacity: 1 }} viewport={{ once: true }}>
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
