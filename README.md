# Audio Spectrum Analyzer

This tool is designed to analyze the frequency characteristics of analog audio devices by generating real-time signals and capturing the audio input. It consists of two main components: a Form that generates signals and sends them to the output of the audio device, and a Spectrum Analyzer (implemented as a user control) that processes the data from the input of the audio device. By connecting an analog device between the input and output of the sound card or audio interface, users can obtain detailed frequency information about the device.

## Features

- **Input Waveform Section**: 
  - Located on the right side of the screen, this section allows users to select various input waveforms including:
    - White Noise
    - Sine
    - Rectified Sine
    - Triangle
    - Square
    - Sawtooth
    - Sine Sweep
    - Clipped Sine
  - Modify the frequency of the input waveform and choose the FFT window type.
  - A record button enables you to capture audio input from the connected audio device.

- **Dual Display**:
  - **Waveform Display**: Shows the input signal from the sound card in the time domain.
  - **Spectrum Display**: Provides the frequency domain representation of the input signal.

- **Marking Points on the Spectrum**:
  - Mark points on the spectrum by double right-clicking on the desired positions.
  - To remove the last marked point, click the ‘Delete Last Point’ button.
  - Points can be added to a 'Marked Points' list for further analysis by clicking ‘Add Points’.
  - Delete points from the list by selecting the desired point and clicking ‘Delete Point’.
  - If more than two points are marked, the last two points form a line, and the slope of that line will be displayed in the top left of the spectrum display, useful for harmonic analysis.

- **Spectrum Display Modes**:
  - Logarithmic X-axis & Logarithmic Y-axis
  - Linear X-axis & Logarithmic Y-axis

- **Envelope Follower**:
  - Adjustable attack and release parameters, allowing you to explore the dynamic response of signals.

- **Filter Section**:
  - Located at the bottom of the spectrum display, the filter section allows you to add filters to the audio signal chain.
  - These filters are useful for modeling the frequency response of analog devices.
  - You can apply the filter effects by enabling the 'Apply Filters' checkbox.
    
## Requirements
- Windows OS
- .NET Framework
- A sound card or external audio device with both input and output capabilities
- **NAudio Library. You should download and include the NAudio project under the NAudio folder from the forked repo. You can find more details and installation instructions** [here](https://github.com/SjB/NAudio).

## Dependencies
This project utilizes the **NAudio** library for handling audio input/output, signal generation, and real-time processing. You can find more details about NAudio and installation instructions [here](https://github.com/SjB/NAudio).

## License
This project is licensed under the MIT License.

## Usage
By connecting an analog device between the input and output of your sound card, this tool helps you visualize the frequency response of the device. Adjust the waveform, mark points, and apply filters to model and analyze the behavior of the analog device in real-time.

## Contributions
Contributions are welcome! If you want to improve this project, feel free to fork the repository, make your changes, and submit a pull request. Please follow the steps below:
- Fork the repository
- Create a new branch (git checkout -b feature/new-feature)
- Commit your changes (git commit -am 'Add some feature')
- Push to the branch (git push origin feature/new-feature)
- Create a new pull request
