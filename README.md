
# Objection

A quick and dirty hotword-based control for Phoenix Wright: Ace Attorney Trilogy on Windows PC. Finally, screaming Objection!, Take That!, and Hold It! becomes a reality for the first time since the DS release.

## Demo video
Coming soon!

## How-To
 - Install the prerequisites as in requirements.txt
	 - This is easiest on Python 3.6, which already has a pre-built wheel for PyAudio.
 - Generate three Porcupine hotword files and put them in the root folder of Objection:
	 - objection_windows.ppn
	 - take_that_windows.ppn
	 - hold_it_windows.ppn
 - Boot up Ace Attorney Trilogy
 - Run objection.py

If all goes well, Objection will hook into the Ace Attorney window and wait for microphone input.

## Binaries
A binary release for amd64-compatible machines will be available soon.

### Building your own binary
 - Follow the 'how-to' steps
 - Modify the Porcupine paths in objection.py as necessary to match your target system
 - Run the following command after also installing pyinstaller: `pyinstaller --add-data "porcupine;porcupine" --add-data "<your python root directory>/lib/site-packages/_soundfile_data;_soundfile_data" --onefile -p porcupine/binding/python objection.py`

## Licenses

The Apache 2.0 licensed portions of the Porcupine library are included in this repository unaltered.

The Objection software itself is licensed under the MIT License, which can be found in the LICENSE file.
