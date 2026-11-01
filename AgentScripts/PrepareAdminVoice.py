"""Prepare the administrator's Abyssal Warden voice using the shared WAV extraction."""
import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
from PrepareDarkRadioVoice import prepare


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path, default=Path(
        'C:/Users/moroz/Desktop/DEV/UNITY/DREAMS-DATABASES/VoiceDatabase/admin_voice/12_Abyssal_Warden.wav'))
    args = parser.parse_args()
    # Low-energy boundaries from 5 ms RMS analysis; retain consonants and short tails.
    regions = [(0.855, 1.500), (1.575, 2.200), (2.205, 2.875),
               (3.735, 4.335), (4.405, 5.220), (5.275, 5.760),
               (6.670, 7.475), (7.490, 8.205), (8.205, 8.935),
               (9.860, 10.590), (10.590, 11.205), (11.265, 12.150),
               (13.350, 14.005), (14.005, 14.685), (14.750, 15.380),
               (16.360, 17.080), (17.080, 17.760), (17.850, 18.310),
               (19.575, 20.430), (20.460, 21.230), (21.250, 21.945)]
    project = Path(__file__).resolve().parent.parent
    prepare(args.source, project / 'Assets/Audio/Voices/AdminAbyssalWarden', 'abyssal_warden', regions)


if __name__ == '__main__':
    main()
