using System;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using UnityEngine;

[Serializable]
public class StepManagerMedCompAssetData : BaseAssetData
{

    /* Checkable Steps: 
     * Numbered steps are what can be measured currently by the program, steps without numbers cannot yet be graded but should be done by student
     *
     *  Read MAR
     *  Check Patient Name
     *  1. Enter Med Room
     *  2. Wash Hands
     *  3. Check the MAR to ensure the correct patient name
     *  4. Check the MAR to ensure to correct medication
     *  Check for each factor (dose, route, exp, etc.)
     *  Go back to patient room
     *  Check patient name and DOB
     *  Check Meds again
     *  5. Wash hands
     *  6. Provide water
     *  7. Watch patient take meds
     *  Educate about meds
     *  Tell patient to call if feeling adverse effects
     *  8. Wash hands
     *  Document Meds
     */

    //The list of steps that can be checked to ensure that the student properly completed them
    public string[] stepList =
    {
        "1. Washed hands when entering med room",
        "2. Checked the MAR to ensure the correct patient name when dispensing meds",
        "3. Checked the MAR to ensure to correct medication when dispensing meds",
        "4. Washed hands when entering patient room",
        "5. Provided patient with medicine",
        "6. Provided patient with water",
        "7. Watched patient take meds and did not leave them unattended",
        "8. Wash hands after providing meds"
    };


    //The booleans that keep track of whether each step was completed
    public bool[] stepCompletedBool;

}
