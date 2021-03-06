flatsch
=======

Office-Eye-Syndrome, dry/hurting eyes, headache etc., when working on the PC?
This could be caused by not blinking often enough [1], [2].

Flatsch reminds you to blink frequently or follow the 20-20-20 rule, which might assist in reducing your pain.

> WARNING: This app may potentially trigger seizures for people with photosensitive epilepsy. Please take care!

Download
--------

You can compile flatsch yourself or get it from one of the following sources:

[Github](https://github.com/jbinder/flatsch/releases/latest) / 
[Softpedia](https://www.softpedia.com/get/Others/Miscellaneous/Flatsch.shtml)

Features
--------

* Throws a fish or just text at your face!
* Multi-monitor support
* Transparent or on white, so it's impossible to overlook
* Customize the fish!
* Customizable timings, colors, transparency, ...
* Has profiles for
  * regular blinking: flashes the screen every 5 seconds
  * 20-20-20 rule [3]: fades in an overlay every 20 minutes, encouraging you to look 20 seconds at something which is 20 feet away
* Create your own profiles

Usage
-----

Just start the Flatsch.exe, quit the app using the tray icon.
To customize the fish, replace the files in the 'res' directory.

Requirements
------------

* Microsoft .NET Framework 4.6.2

To build the app:

* Visual Studio 2017
  * .NET desktop development workload

Changelog
---------

### [0.4]

- Add default profiles: 20-20-20 rule, regular blinking
- Support adding/modifying/removing of custom profiles
- Animation improvements
- Visual improvements

### [0.3]

- Add Multi-monitor support
- Animation improvements

### [0.2]

- Add settings dialog for customizing blink reminder style and timings
- Allow the app to be paused
- Visual improvements

### [0.1]

- Initial release, basic blink reminder functionality

Thanks
------

* Icon gfx: https://pixabay.com/en/eye-warm-angry-young-fashion-1456855/
* Fish gfx: https://openclipart.org/detail/3675/fish-in-ink
* Fish sound: https://freesound.org/people/qubodup/sounds/222507/
* Tray icon lib: https://www.codeproject.com/Articles/36468/WPF-NotifyIcon
* Color picker: https://github.com/drogoganor/ColorPickerWPF

References
----------

* [1] https://en.wikipedia.org/wiki/Dry_eye_syndrome
* [2] https://www.ncbi.nlm.nih.gov/pubmed/22710496
* [3] https://www.aoa.org/patients-and-public/caring-for-your-vision/protecting-your-vision/computer-vision-syndrome
