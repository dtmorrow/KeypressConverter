# KeypressConverter
This application will convert a key press that has no scan code (meaning it is most likely from a programmable mouse or keyboard) to a key press with the same scan code as its corresponding virtual key. It also allows for an optional delay for the KeyUp event to hold the key down longer than it is actually pressed. This application is useful for games that read their inputs based on scan codes only. You can read more about the motivation and development of this application in the [Development Notes](DevNotes.md)

## Requirements
Microsoft .NET Framework 4.5.2 or higher.

## Usage
When run when no KeyConfig.txt file is in the same directory as the executable will create a default KeyConfig.txt file.
When run with a valid KeyConfig.txt file in the same directory as the executable, the application will convert keys while the application remains open. Press ENTER to close the application.

The format of KeyConfig.txt consists of the name of the key as defined by the .NET Keys enum, followed by an optional delay, separated by a space. Each key should be on a separate line. Single line comments can be used by using a '#' character at the beginning of the line.

### Example
#### Contents of KeyConfig.txt
```
# These keys will be converted 
Enter
Left 10
Right 10
Space
```
This will convert the Enter, Space, and Left and Right arrow keys to key presses with scan codes. Left and Right will have an additional 10 ms delay on their KeyUp events.

## Building
Solution requires Visual Studio 2015.
