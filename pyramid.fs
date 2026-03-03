FeatureScript 2126;
import(path : "onshape/std/geometry.fs", version : "2126.0");

/**
 * Pyramid Feature for Onshape
 *
 * Creates a solid pyramid from a user-selected planar face (the base) and a
 * specified height.  The apex is located at the bounding-box centre of the
 * base face (the "best-guessed midpoint"), offset along the face normal by
 * the given height.  Every lateral face of the pyramid is a flat triangle
 * that connects one base edge to the apex.
 *
 * Usage
 * -----
 *  1. Draw a closed sketch that defines the desired base shape (triangle,
 *     square, pentagon, irregular polygon, etc.).
 *  2. Insert the Pyramid custom feature and select the sketch region – or any
 *     other flat/planar face – as the "Base profile".
 *  3. Enter the desired "Height".
 *  4. If the pyramid grows in the wrong direction, toggle "Flip direction".
 */

/** Allowable range and default value for the pyramid height. */
const PYRAMID_HEIGHT_BOUNDS =
{
    (meter)      : [0.0001, 0.05,  10],
    (centimeter) : [0.01,   5.0,  1000],
    (millimeter) : [0.1,   50.0, 10000],
    (inch)       : [0.004,  2.0,   393],
    (foot)       : [0.0003, 0.17,    33],
    (yard)       : [0.0001, 0.056,   11]
} as LengthBoundSpec;

annotation { "Feature Type Name" : "Pyramid" }
export const pyramid = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        // The base of the pyramid – any flat (planar) face, including sketch regions.
        annotation { "Name" : "Base profile",
                     "Filter" : EntityType.FACE && GeometryType.PLANE,
                     "MaxNumberOfPicks" : 1 }
        definition.baseProfile is Query;

        // How tall the pyramid should be.
        annotation { "Name" : "Height" }
        isLength(definition.height, PYRAMID_HEIGHT_BOUNDS);

        // Reverses the build direction (apex goes to the opposite side of the base).
        annotation { "Name" : "Flip direction",
                     "UIHint" : UIHint.OPPOSITE_DIRECTION }
        definition.flipDirection is boolean;
    }
    {
        // ── Validate selection ────────────────────────────────────────────────
        if (isQueryEmpty(context, definition.baseProfile))
        {
            throw regenError("Select a planar face as the pyramid base.", ["baseProfile"]);
        }

        // ── Determine the build direction (outward face normal) ───────────────
        //    evFaceTangentPlane returns a Plane whose normal equals the face
        //    normal at the given UV parameter.  For a planar face the normal is
        //    constant everywhere, so any valid UV point will do.
        var faceTangent = evFaceTangentPlane(context, {
                "face"      : definition.baseProfile,
                "parameter" : vector(0.5, 0.5)
            });

        var normal = definition.flipDirection ? -faceTangent.normal : faceTangent.normal;

        // ── Estimate the base centroid via the axis-aligned bounding-box ──────
        //    For convex, regular, or near-regular base shapes this gives an
        //    excellent "best-guessed midpoint" as described in the feature spec.
        var bbox = evBox3d(context, {
                "topology" : definition.baseProfile
            });

        var baseCentre = (bbox.minCorner + bbox.maxCorner) * 0.5;

        // ── Apex position ─────────────────────────────────────────────────────
        var apexPosition = baseCentre + definition.height * normal;

        // ── Build an auxiliary sketch that holds a single point at the apex ───
        //    opLoft accepts a lone vertex as a degenerate profile, which causes
        //    every edge of the base to converge to that single point – exactly
        //    the geometry of a pyramid.
        var apexSketch = newSketchOnPlane(context, id + "apexSketch", {
                "sketchPlane" : plane(apexPosition, normal)
            });

        skPoint(apexSketch, "apex", { "position" : vector(0, 0) * meter });

        skSolve(apexSketch);

        // ── Loft: base face → apex vertex → solid pyramid ─────────────────────
        opLoft(context, id + "pyramid", {
                "profileSubqueries" : [
                    definition.baseProfile,
                    qCreatedBy(id + "apexSketch", EntityType.VERTEX)
                ],
                "bodyType" : ToolBodyType.SOLID
            });

        // ── Remove the auxiliary apex sketch (construction geometry only) ─────
        opDeleteBodies(context, id + "deleteApex", {
                "entities" : qCreatedBy(id + "apexSketch", EntityType.BODY)
            });
    });
