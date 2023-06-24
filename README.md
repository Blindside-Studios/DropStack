# DropStack
An emulation of the dock stacks feature from MacOS, built with the UWP framework

![Screenshot of DropStack on the Windows desktop](DropStack/Assets/Product%20Screenshots/ondesktop.png)

## DropStack's basics
DropStack's main goal is to help you reach the most recently added or modified file in your downloads folder or any folder that you choose. In addition to this, DropStack also offers a feature that allows you to pin any files you like, in order to quickly access them later on.

## DropStack's audience
We know that opening File Explorer, choosing the downloads folder and then finding the file you are looking for is and will always be a valid way to access your files. However, for some users, especially those that use their device with a touch screen, this might be very cumbersome. DropStack offers a simpler and more comfortable way to access these files.

## How it works
### Getting started
On first launch, DropStack will greet you and allow you to pick two unique locations. The first one will be used for its Portal feature, which works similarly to macOS' dock stacks, and the second one will be used to store your pinned files.

![How to choose the download folder as portal folder](DropStack/Assets/Product%20Screenshots/pickportal.png)

Remember that DropStack copies your pinned files to this location, meaning that you can move or even delete the original without it disappearing from your DropStack pinned library.

And that's it, you're done with the setup!

![Screenshot of the finished out of box experience](DropStack/Assets/Product%20Screenshots/setupdone.png)
![Screenshot of the app window](DropStack/Assets/Product%20Screenshots/window.png)

### Navigation
DropStack is categorized into multiple views: Portal View and Pinned View.
You can switch through them any time by either clicking the appropriate header at the top left corner or by swiping between them with your finger - and of course you can also scroll horizontally on your trackpad.

![Screenshot of the Pinned section in DropStack](DropStack/Assets/Product%20Screenshots/pinnedfiles.png)

### File Interactions
Once you located the file of your desire, you have three main ways of interacting with it:
- Click: Clicking on the file's name will open the file.
- Right-Click: Right-clicking (or long-pressing) on the file will copy the file to your clipboard, indicated through a small flyout, which appears on the bottom corner of the app.
- Drag: Next to opening and copying the file, DropStack also allows you to drag your file anywhere - this means you can drop the file into another app or your folder or even your desktop. If you are using a touch screen, you can long-press the file and then drag it out.

![Successful copy](DropStack/Assets/Product%20Screenshots/copysuccess.png)
![Dragging a file into a Discord chat](DropStack/Assets/Product%20Screenshots/dragdrop.png)

### Navigation Bar
To the right of the view headers, you will find three buttons. These are:
- Reveal: This reveals the folder you are viewing in File Explorer
- Refresh: You updated a file but the update doesn't show? Click this button or alternatively pull down to refresh
- Meatball Menu: This menu allows you to execute various other commands, this currently includes opening the settings pane, copying the most recent file and switching to simple mode (more on simple mode later).

![Screenshot of the command bar](DropStack/Assets/Product%20Screenshots/meatballmenu.png)

Note that all of these commands can be executed via keyboard shortcuts. Here is the current list of keyboard shortcuts:
- Ctrl+E: Reveal in File Explorer
- Ctrl+R: Refresh this page
- Ctrl+I: Open settings
- Ctrl+C: Copy recent from portal
- Ctrl+S: Launch simple mode...

### Simple mode
Simple mode displays less items with larger icons, which is perfect if you are using a tablet or don't want to be overwhelmed by a large list of files. Its functionality is exactly the same as the more detailed view's.

![Screenshot of Simple mode](DropStack/Assets/Product%20Screenshots/simplemode.png)

### Settings
Settings allows you to tweak the app's behavior. To elevate the discoverability and reducing complexity, each toggle is ToolTip-enabled, meaning that holding your mouse cursor over a toggle gives a short explanation of what is about to change, after clicking the toggle.
Here are the settings that are currently available (this list will be expanded as new features are being added):
- Use simple view by default: Allows you to toggle between complex and simple view. This will reflect in the page that is being disaplayed in the background instantly.
- Revoke Folder Access: Lets you disconnect the app from the folder you are currently viewing, whether it is the portal folder or the pinned folder. This requires a confirmation. Disconnecting either folder will restart the app, disconnecting both will guide you through the initial setup again. Note that none of your files are being lost, they are just not accessible in your app anymore. This is helpful in the case that you would like to pick another folder.

![Screenshot of the settings panel](DropStack/Assets/Product%20Screenshots/settings.png)

## Update policy
We plan to frequently update DropStack with new features and bug fixes, however, we do not provide any legal binding that entitles you to updates.

## Feedback
If you would like to provide feedback, feel free to do so. We are always happy to listen to your feature suggestions and other things you would like to see changed. If you want to report a bug or suggest a feature, please issue a GitHub ticket. We also plan to add an integrated feedback system into the app down the line.
