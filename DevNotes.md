# Development Notes
This page serves as a document for my motivation behind creating this project and the process I went through creating it.

## The Problem
The need for this application came about when I was attempting to get my programmable mouse to work with a game. The mouse I was using has 8 programmable buttons: left, middle, and right mouse buttons, a 4th additional mouse button to the right of the right mouse button, left and right tilts on the mouse wheel, and forward and back buttons. I was attempting to map the left and right mouse wheel tilt to the left and right arrow keys, and the forward and back buttons to Space and Enter.

The game was not responding to the mouse input, though, even though the remapped buttons were working correctly with other applications. After some searching and experimentation, I discovered what the issue was.

## Keyboard Input Model
Microsoft's Dev Center has a detailed description of Window's keyboard input model. [Source](https://msdn.microsoft.com/en-us/library/windows/desktop/ms646267(v=vs.85).aspx)
>Assigned to each key on a keyboard is a unique value called a scan code, a device-dependent identifier for the key on the keyboard. A keyboard generates two scan codes when the user types a key—one when the user presses the key and another when the user releases the key.

>The keyboard device driver interprets a scan code and translates (maps) it to a virtual-key code, a device-independent value defined by the system that identifies the purpose of a key. After translating a scan code, the keyboard layout creates a message that includes the scan code, the virtual-key code, and other information about the keystroke, and then places the message in the system message queue.

Since an application receives both the scan code and virtual key in the message generated from a keyboard event, it can choose to respond to either. Most applications read the virtual key (.NET's Console.ReadKey and Forms.Control.KeyUp, KeyPress, and Keydown events all provide the virtual key information), however an application can, as was the case with the game in question, react to input based on the scan code.

I created a Keyboard Hook to listen for keyboard events, and after intercepting a keyboard event from my mouse, saw that the virtual key was mapped correctly, but the scan code was zero. If I was going to have the game respond to my remapped mouse keys, I would have to change the keyboard event to include the correct scan code.

## Converting Virtual Keys to Scan Codes
The Windows API has a function called MapVirtualKey that can convert a virtual key to its matching scan code. I was hoping I could intercept the keyboard event, modify the scan code to correct code, then pass the keyboard event on. I couldn't seem to get this to work, however. I suspect the game was either getting the event before my application could modify it, or it was still receiving the old keyboard event even though I was attempting to modify it. I wasn't able to determine the exact reason why that method wouldn't work, but I did come up with another solution. I would suppress the original keyboard event, then output a new keyboard event with the same information and correct scan code using the SendInput function.

After setting up the Keyboard Hook callback to wait for a keyboard event with a scan code of zero, suppress the keyboard event, and create a new one with correct scan code, the game finally started to respond to my remapped keys! ...Mostly. The Forward (Space) and Back (Enter) buttons were working, but the left and right tilts still weren't being registered as left and right arrow keys.

## Methods of Processing Input
When an application receives a keyboard event, there are multiple ways to process the event. Most applications, as detailed in the keyboard input model earlier, process the events from a message queue as they are able. The game wasn't doing it this way, however. Instead, as far as I can tell experimentally, each frame the game checks what keys are being held down, then processes them. This means that, theoretically, you could press a key faster than the game could register if you pressed and released the key in-between frames. This doesn't need to be theoretical, though, since that seems to be exactly what was happening.

For the left and right tilts on the mouse wheel, instead of sending a key down event when the wheel is tilted and a key up event when it is released, the mouse: sends a key down event, followed by a very short delay, followed by a key up event, and it continually does this as long as wheel is tilted.

I checked to see how long the tilt keys were being "held down" for using [this site](http://blog.seethis.link/scan-rate-estimator/).

The results of a single left tilt in the "Key press history" looked similar to this:

>left arrow : 10ms  
>left arrow : 10ms  
>left arrow : 10ms  
>left arrow : 10ms  
>left arrow : 11ms  

As you can see, the keys were only being held down for around 10 ms. The game I was playing was running at 60 frames-per-second, or 1 frame about every 17 ms. So the keys actually *were* being held down for less time than the game could read them.

To fix this, I decided to add a 10 ms delay for the key up event for those specific keys: long enough so that the game could read the keys, but still short enough that there isn't any noticable (for me at least) latency between keyboard events. After doing this, the game was reading all the remapped keys correctly.

## Extra Features
I decided to make the application more general purpose in case I needed to reuse this technique for other games. This included having the application read the keys needed to process with optional delays from a text file so that keys that wouldn't need to be modified aren't unnecessarily suppressed and re-outputted (I'm sure there's a very small amount of latency even if you don't delay the key up event).
