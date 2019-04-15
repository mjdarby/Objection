# pyinstaller --add-data "porcupine;porcupine" --add-data "venv3.6/lib/site-packages/_soundfile_data;_soundfile_data" --onefile -p porcupine/binding/python objection.py

import pywinauto
import os
import sys
import struct

import pyaudio

PORCUPINE_HOME = os.path.join(os.path.dirname(os.path.abspath(__file__)), "porcupine")
PORCUPINE_BINDING = os.path.join(PORCUPINE_HOME, "binding", "python")
PORCUPINE_ARCH1 = "windows"
PORCUPINE_ARCH2 = "amd64"

app = None
porcupine = None
pa = None
audio_steam = None

OBJECTION = 0
TAKE_THAT = 1
HOLD_IT = 2

sys.path.append(PORCUPINE_BINDING)
from porcupine import Porcupine

def objection():
    app.top_window().type_keys("{e down}")
    app.top_window().type_keys("{e up}")

def takethat():
    app.top_window().type_keys("{e down}")
    app.top_window().type_keys("{e up}")

def holdit():
    app.top_window().type_keys("{q down}")
    app.top_window().type_keys("{q up}")

def connectPhoenixWindow():
    app = pywinauto.application.Application()
    app.connect(title_re="Phoenix Wright: Ace Attorney Trilogy")
    return app

def run():
    try:
        while True:
            pcm = audio_stream.read(porcupine.frame_length, exception_on_overflow=False)
            pcm = struct.unpack_from("h" * porcupine.frame_length, pcm)

            result = porcupine.process(pcm)
            if result == OBJECTION:
                print("OBJECTION!")
                objection()
            if result == TAKE_THAT:
                print("TAKE THAT!")
                takethat()
            if result == HOLD_IT:
                print("HOLD IT!")
                holdit()

    except KeyboardInterrupt:
        print('stopping ...')
    finally:
        if porcupine is not None:
            porcupine.delete()

        if audio_stream is not None:
            audio_stream.close()

        if pa is not None:
            pa.terminate()

if __name__ == "__main__":
    porcupine = Porcupine(
        library_path = os.path.join(PORCUPINE_HOME, "lib", PORCUPINE_ARCH1, PORCUPINE_ARCH2, "libpv_porcupine.dll"),
        model_file_path=os.path.join(PORCUPINE_HOME, "lib", "common", "porcupine_params.pv"),
        keyword_file_paths=[os.path.join(os.path.dirname(os.path.abspath(__file__)), "objection_windows.ppn"),
                            os.path.join(os.path.dirname(os.path.abspath(__file__)), "take_that_windows.ppn"),
                            os.path.join(os.path.dirname(os.path.abspath(__file__)), "hold_it_windows.ppn")],
        sensitivities=[0.5, 0.5, 0.5])

    pa = pyaudio.PyAudio()
    audio_stream = pa.open(
        rate=porcupine.sample_rate,
        channels=1,
        format=pyaudio.paInt16,
        input=True,
        frames_per_buffer=porcupine.frame_length,
        input_device_index=None)

    app = connectPhoenixWindow()

    run()
