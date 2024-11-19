using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

/* Script Dependencies
 * 
 * Chlorhexadine relies on StepManager
 * HandDetector relies on Chlorhexadine
 * 
 */

/* STEPS
 * 1. Peel off old tegaderm. Make sure catheter stays secure in doing so.
 * 2. Open supplies package (ensure things stay sterile)
 * 3. Grab chlorhexadine applicator, squeeze "wing" tabs until they release the chlorhexadine into the sponge.
 * 4. Apply chlorhexadine using applicator to area around catheter. DO NOT BLOW ON OR WIPE CLEAN, LET IT AIR DRY!
 * 5. Grab new tegaderm, peel of wrapping on the sticky side.
 * 6. Stick tegaderm over the catheter area.
 * 7. Peel off outline wrapping from tegaderm.
 * 
 * 1 check for each part of step 1.
 * 1 check for step 5.
 * 1 check for first part of step 6, 1 for not wiping clean, (use multiple trigger zones around catheter).
 * 1 check for step 7.
 * 1 check for step 8.
 * 1 check for step 9.
 * 8 checks total.
 * 
 */

[Serializable]
public class StepManagerAssetData : BaseAssetData
{
    public static readonly string[] FAILED_CHECK_TEXT = { "You did not remove the old tegaderm.",
        "You did not keep the catheter secure while removing the old tegaderm.",
        "You did not release the chlorhexadine into the sponge of the applicator.",
        "You did not wipe all of the area around the catheter with the chlorhexadine applicator.",
        "You did not allow the chlorhexadine to air dry.",
        "You did not remove the paper covering the sticky side of the new tegaderm.",
        "You did not apply the new tegaderm to the catheter area.",
        "You did not remove the paper outlining the topside of the tegaderm.",};

    public static readonly string[] FAILED_ORDER_TEXT = {
        "You did not remove the old tegaderm before using the chlorhexadine applicator.",
        "You did not remove the old tegaderm before applying the new tegaderm.",
        "You did not release the chlorhexadine into the sponge of the applicator before using it.",
        "You did not fully disinfect the area around the catheter before applying the new tegaderm.",
        "You did not remove the paper covering the sticky side of the new tegaderm before trying to apply it.",
        "You did not apply the new tegaderm before removoing the paper outlining its topside.",};

    public static readonly string[] FAILED_OTHER_TEXT = {
        "You did not allow the chlorhexadine to air dry (don't use the gauze).",
        "You used the alcohol swab to disinfect the catheter area instead of the chlorhexadine applicator." };

    /* THE CHECKS ARE FOR AS FOLLOWS
     * 
     * X[0] Peel off old tegaderm.
     * X[1] Keeping catheter secure while peeling off old tegaderm.
     * X[2] Squeezing "wing" tabs of chlorhexadine applicator until they release the chlorhexadine into the sponge.
     * X[3] Applying chlorhexadine to the area around the catheter.
     *  [4] Letting the chlorhexadine air dry.
     * X[5] Peeling off wrapping covering sticky side of new tegaderm.
     *  [6] Sticking tegaderm to catheter area.
     * X[7] Peeling off outline wrapping from tegaderm.
     */
    public AssetPropertyDefinition<bool[]> stepChecks;

    //Keeps track of the order in which stepChecks were triggered.
    /* Order Checks
     * [0] in FAILED_ORDER_TEXT: Doing [3] before [0]
     * [1] in FAILED_ORDER_TEXT: Doing [6] before [0]
     * [2] in FAILED_ORDER_TEXT: Doing [3] before [2]
     * [3] in FAILED_ORDER_TEXT: Doing [6] before [3]
     * [4] in FAILED_ORDER_TEXT: Doing [6] before [5]
     * [5] in FAILED_ORDER_TEXT: Doing [7] before [6]
     */
    public AssetPropertyDefinition<int[]> attemptedOrder;

    //Mistakes from Other Prop Use
    /* [0] Using the gauze to dry off the catheter area.
     * [1] Using the alcohol swab the disinfect the area instead of the chlorahexadine applicator.
     */
    public AssetPropertyDefinition<bool[]> otherMistakes;

    //A compilation of text from the various above arrays to be displayed after a simulated session.
    public AssetPropertyDefinition<string> mistakeText;
}
