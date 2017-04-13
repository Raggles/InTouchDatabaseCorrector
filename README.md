InTouchDatabaseCorrector
==================================

Attempts to autocorrect corrupt InTouch Tag Databases

Will currently correct the following:

* Initial Values that are larger or smaller than the engineering limits
* Alarms (LoLo, Lo, Hi, HiHi) that overlap, or are outside engineering limits
* Initial Values and Minor Alarm Deviations that are very small
* Removal of tags with bad AccessNames

More features will be added as required.

