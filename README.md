# featurescript-test

An Onshape FeatureScript custom feature that creates a solid **pyramid** from any flat sketch profile and a user-specified height.

---

## File

| File | Description |
|------|-------------|
| `pyramid.fs` | FeatureScript source – import this into your OnShape document |

---

## How to use in Onshape

1. Open (or create) an Onshape document and go to a **Part Studio**.
2. Click the **"+"** button in the feature toolbar → **"Add custom features"**.
3. Paste or link to `pyramid.fs` and confirm.
4. Draw a **closed sketch** that defines the base shape of the pyramid  
   (triangle, square, pentagon, irregular polygon, etc.) and exit the sketch.
5. Insert the **Pyramid** feature from the toolbar.
6. **Base profile** – click the closed sketch region (or any other flat face) in the viewport.
7. **Height** – enter the desired pyramid height.
8. Optionally toggle **Flip direction** if the apex appears on the wrong side.
9. Click the green check-mark to confirm.

The feature produces a solid body whose lateral faces are flat triangles that all meet at a single apex. The apex is placed at the **bounding-box centre** of the base face (the best-guessed midpoint), offset by the specified height along the face normal.

---

## Parameter reference

| Parameter | Type | Description |
|-----------|------|-------------|
| Base profile | Face query | Any planar face – typically a closed sketch region |
| Height | Length | Distance from the base plane to the apex (default 50 mm) |
| Flip direction | Boolean | Reverses the apex direction relative to the base normal |

---

## Requirements

* Onshape FeatureScript **2126** (or later compatible version)
* Standard geometry library `onshape/std/geometry.fs` version **2126.0**
