# 🎮 SLIME VIRTUAL PET - SETUP GUIDE

## 📋 Quick Setup Instructions

### Step 1: Create the Slime GameObject

1. Create an **empty GameObject** in your scene: `GameObject → Create Empty`
2. Name it **"Slime"**
3. Set its **position** to `(0, 1, 0)` and **scale** to `(2.5, 2.5, 2.5)`

### Step 2: Add the Mesh

#### Option A: Auto-Generate (Recommended)
1. Add the `JellySphereGenerator` component to your Slime GameObject
2. Set **Subdivisions** to `3` (creates ~500 vertices for good jelly physics)
3. Set **Radius** to `1`
4. Click the context menu (⋮) and select **"Generate Sphere"**

#### Option B: Use Unity Sphere
1. Create a sphere: `GameObject → 3D Object → Sphere`
2. Make it a child of your Slime GameObject
3. Set its local position to `(0, 0, 0)`

### Step 3: Add All Scripts

Add these components to your **Slime GameObject** (in this order):

1. **JellyMesh** ✅ (Core physics)
2. **SlimeFaceController** 👀 (Eyes and emotions)
3. **ParticleController** ✨ (Visual effects)
4. **TouchInteractionSystem** 🖱️ (Input handling)
5. **SlimeController** 🎮 (Main coordinator)

### Step 4: Create and Assign Material

1. Create a new Material: `Right-click in Project → Create → Material`
2. Name it **"SlimeMaterial"**
3. Change shader to: **Custom/SlimeGlow**
4. Assign this material to the Slime's **MeshRenderer**

### Step 5: Configure the Material

Set these material properties for a cute glowing slime:

**Main Color:**
- Color: Cyan/Green `(0.5, 1, 0.8)` or Pink `(1, 0.5, 0.8)`
- Alpha: `0.85`

**Emission Glow:**
- Emission Color: Match main color
- Emission Intensity: `2-3`

**Fresnel Rim Light:**
- Fresnel Power: `3`
- Fresnel Intensity: `1.5`

**Subsurface Scattering:**
- SSS Intensity: `0.8`
- SSS Power: `4`

**Interior Detail:**
- Noise Scale: `3`
- Noise Intensity: `0.15`

### Step 6: Adjust Camera

Select your **Main Camera** and set:
- **Position:** `(0, 1.5, -4)`
- **Rotation:** `(10, 0, 0)` (slight downward angle)
- **Field of View:** `45`

Optional: Add **Post Processing** for extra polish:
- Bloom (intensity: 0.5-1)
- Depth of Field
- Color Grading (warm tone)

### Step 7: Configure Components

#### JellyMesh Settings:
- **Stiffness:** `50` (lower = more jiggly)
- **Damping:** `5` (higher = less bouncy)
- **Mass:** `0.3`
- **Touch Force Multiplier:** `5`
- **Touch Force Radius:** `0.3`

#### SlimeFaceController:
- Eyes will auto-create on Start
- **Eye Spacing:** `0.3`
- **Blink Interval:** `2-5` seconds
- **Look at Camera:** ✅ Enabled

#### TouchInteractionSystem:
- **Tap Force:** `3`
- **Hold Force:** `1.5`
- **Drag Force:** `2`

#### ParticleController:
- Will auto-create particle systems
- **Sparkle Color:** Match slime color
- **Heart Color:** Pink `(1, 0.3, 0.5)`

#### SlimeController:
- **Glow Color:** Match material color
- **Breathe Speed:** `1.5`
- **Idle Wobble Interval:** `3` seconds

### Step 8: Add Collider (Important!)

The slime needs a collider for touch interaction:
1. Add `MeshCollider` component to Slime
2. Enable **Convex**
3. Or use `SphereCollider` for better performance

### Step 9: Test It!

Press **Play** and:
- Click/tap the slime → Should deform and bounce with particles!
- Watch it breathe and wobble idle
- Eyes should blink and follow camera
- Sparkles and effects should appear on touch

---

## 🎨 Customization Ideas

### Change Slime Color:
```csharp
// Access from another script:
slimeController.SetGlowColor(Color.magenta);
```

### Change Emotion:
```csharp
slimeController.SetEmotion(SlimeFaceController.SlimeEmotion.Excited);
```

Available emotions: `Happy`, `Excited`, `Sleepy`, `Curious`, `Playful`, `Sad`

### Add Audio:
1. Find cute squishy sound effects
2. Assign to `TouchInteractionSystem → Squish Sounds` array
3. Add happy chirp sounds to `Happy Sounds` array

---

## 🐛 Troubleshooting

**Slime doesn't deform:**
- Check that `JellyMesh` component is attached
- Verify mesh has enough vertices (200+ minimum)
- Make sure collider is attached

**Eyes not appearing:**
- `SlimeFaceController` auto-creates eyes on Start
- Check Console for errors
- Manually assign eye transforms if needed

**Touch not working:**
- Verify collider is on slime GameObject
- Check camera is assigned to TouchInteractionSystem
- Ensure slime layer matches raycast layers

**No particles showing:**
- Particle systems auto-create on Start
- Check if `ParticleController` is attached
- Verify material is set to additive blend

**Jelly physics unstable/exploding:**
- Reduce `Stiffness` (try 30-40)
- Increase `Damping` (try 7-10)
- Lower `Touch Force Multiplier` (try 3)

---

## 📊 Performance Tips

For better performance:
- Use `Subdivisions = 2` (80 triangles) instead of 3
- Disable ambient particles if frame rate drops
- Use `SphereCollider` instead of `MeshCollider`
- Reduce particle emission counts

For mobile:
- Target 30 FPS
- Use 200-300 vertices maximum
- Lower particle counts by 50%
- Disable subsurface scattering in shader

---

## 🎯 Next Steps

Want to add more features?

1. **Stats System** - Add hunger, happiness, energy
2. **Feeding** - Drag food items to slime
3. **Accessories** - Hats, bows, glasses
4. **Mini-games** - Bounce ball, catch items
5. **Save/Load** - Persistent pet state
6. **Multiple Slimes** - Different colors and personalities

---

## 📝 Component Summary

| Component | Purpose | Required |
|-----------|---------|----------|
| JellyMesh | Realistic jelly physics | ✅ Yes |
| SlimeFaceController | Eyes and emotions | ✅ Yes |
| ParticleController | Visual effects | ✅ Yes |
| TouchInteractionSystem | Input handling | ✅ Yes |
| SlimeController | Main coordinator | ✅ Yes |
| JellySphereGenerator | Mesh creation | Optional |
| MeshCollider/SphereCollider | Touch detection | ✅ Yes |

---

Enjoy your adorable jelly slime virtual pet! 💚✨
