# Bart's Coding Challenges

<!-- 
  Mark tasks as completed with [x] when done.
  The coding-challenge agent picks the first unchecked task.
-->

## Challenges

- [x] **Read Level Meter from Mixer via OSC**
  A Meter represents the live volume of a channel in the mixer. The mixer is able to send meter values (e.g. volume of an input channel) from the mixer to clients via OSC. Code lines 53 and following in 'Channel.cs' already contain a starting point of how you can read the meter value from the mixer. Your task is to implement this functionality and write unit tests to verify it works correctly. The meter value should be updated in real-time as the mixer sends new values. As a result, the channel class shall invoke a new event `MeterValueUpdated` whenever a new meter value is received from the mixer. This event should provide the updated meter value to any subscribers.
- [x] **Find a suitable way to show the meter value in the plugin UI**
  The plugin UI is currently very basic and only shows the channel name and fader level. Your task is to find a suitable way to also display the meter value in the UI. This could be a simple numeric display, a progress bar, or any other visual representation that effectively conveys the current meter value. Implement this in the plugin UI and ensure it updates in real-time as new meter values are received from the mixer. Write unit tests to verify that the UI correctly reflects changes in the meter value.
