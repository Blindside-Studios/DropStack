# DropStack
Playing hide-and-seek with your files was never an option!

![Screenshot of DropStack on the Windows desktop](Screenshots/v0.4.0/ondesktop.png)

## DropStack's basics
DropStack's main goal is to help you reach the most recently added or modified file in your downloads folder or any folder that you choose. In addition to this, DropStack also offers a feature that allows you to pin any files you like, in order to quickly access them later on.

## DropStack's audience
We know that opening File Explorer, choosing the downloads folder and then finding the file you are looking for is and will always be a valid way to access your files. However, for some users, especially those that use their device with a touch screen, this might be very cumbersome. DropStack offers a simpler and more comfortable way to access these files.

## How it works
### Getting started
On first launch, DropStack will greet you and allow you to pick two unique locations. The first one will be used for its Portal feature, which works similarly to macOS' dock stacks, and the second one will be used to store your pinned files.

Remember that DropStack copies your pinned files to this location, meaning that you can move or even delete the original without it disappearing from your DropStack pinned library.

And that's it, you're done with the setup!

### Navigation
DropStack unifies your portal files and your pinned files into one view.
At the top, you can find your pinned files in the expander. You can click the header to expand or collapseyour pinned files. To add files, just drag them into this section. To delete them, drag them out of these section into the recycle bin on your desktop.

![Screenshot of the Pinned section in DropStack](Screenshots/v0.4.0/pinnedfiles.png)

### File Interactions
Once you located the file of your desire, you have three main ways of interacting with it:
- Double-Click: Double-clicking on the file's name will open the file.
- Right-Click: Right-clicking (or long-pressing) on the file will copy the file to your clipboard, indicated through a small flyout, which appears on the bottom side of the app.
- Drag: Next to opening and copying the file, DropStack also allows you to drag your file anywhere - this means you can drop the file into another app or your folder or even your desktop. If you are using a touch screen, you can long-press the file and then drag it out.

![Dragging a file into a Discord chat](Screenshots/v0.2.0/dragdrop.png)

This applies to all files in DropStack.

### Navigation Bar
At the top of the window, you will find multiple buttons. These are:
- Search: This lets you search through all of your files. Search is not case sensitive, so you do not need to remember your own or that website's spelling conventions.
- Copy: This button allows you to single-Click a file and then copy it to the clipboard - this is mainly meant for tablet and laptop users who do not have a proper right mouse button. Please note that the app must remain open until the file has been pasted at least once.
- Refresh: You updated a file but the update doesn't show? Just click this button.
- Reveal: This reveals the portal folder inside of File Explorer in case you need to take more advanced actions with your files.
- Simple: This launched simple mode (more on simple mode later).
- Copy newest: This copies the most recently modified file in your portal folder.
- Open: This opens the last selected file and is intended for people who have difficulties with the fast action of double-clicking or double-tapping. DropStack's mission is to make it easier to get to your files, for everyone.
- Meatball Menu: The meatball menu houses all the items that did not fit on the top bar of the window, plus settings and the about DropStack section, which includes general app info and a [privacy statement](https://github.com/Blindside-Studios/DropStack/main/README.md#privacy-statement), explaining our no-telemetry policy.

![Screenshot of the command bar](Screenshots/v0.4.0/meatballmenu.png)

The wider your window is, the more buttons will be shown at the top of the app. However, we still recommend to keep the window as narrow as possible.

Note that all of these commands can be executed via keyboard shortcuts. Here is the current list of keyboard shortcuts:
- Ctrl+F: Search
- Ctrl+C: Copy last selected
- Ctrl+R: Refresh
- Ctrl+E: Reveal in File Explorer
- Ctrl+S: Launch simple mode
- Ctrl+X: Copy newest from portal
- Ctrl+A: Open last selected
- Ctrl+I: Settings
 
We put great emphasis on choosing the keyboard shortcuts with the perfect balance of making sense and beaing neatly organized around your left hand and in close proximity to the left control key.

### Simple mode
Simple mode recently received a brand new rework with the arrival of v0.4.0!

![Screenshot of simple mode](Screenshots/v0.4.0/ondesktopsimple.png)

Buttons that cluttered up the interface have been reduced to a minimum, to the point where the top row just shows the folder name of either the portal folder or the pinned folder you choose at setup, as well as two buttons, one to launch the full DropStack experience, and another one to dismiss the flyout-like experience.

In the row below, you will find eight filter options. @ItsEeleeya proposed this idea in the form of a mockup, and we agreed - adding a search bar to simple mode wouldn't fit its philosophy of being simple and intuitive. Instead, we added a row of filter options. Currently, these include:
- All
- Pinned
- Documents
- Pictures
- Music
- Videos
- Applications
- Presentations

Of course, this filter row has been implemented with the usual standards of DropStack intuitiveness. Clicking on a selected filter when it's active will deselect it, while clicking on All or Pinned when active refreshes the according list. Right-clicking either of the two will reveal the folder in file explorer. And of course you can pin files by dragging them onto the "Pinned" button.

Below this row, you can find the familiar file experience with a twist: file types and sizes are now displayed inside of pills, to gently bring your attention towards them, rather than driving it all over the place with a non-simple interface that threw it all at you. If DropStack recognizes a file type, it will automatically add a descriptive name into the pill, together with the actual file extension. These are also used for sorting.

You will also find that after opening a file or dropping it somewhere, the experience will automatically slide back down to where it came from, leaving you with as much space as possible for your work.

### Settings
Settings allows you to tweak DropStack's behavior. To elevate the discoverability and reducing complexity, each toggle is ToolTip-enabled, meaning that holding your mouse cursor over a toggle gives a short explanation of what is about to change, after clicking the toggle.

![Screenshot of the settings panel](Screenshots/v0.4.0/settings.png)

Here are the settings that are currently available:
- Use simple view by default: Allows you to toggle between normal and simple mode.
- Theme: DropStack lets you customize the background with six themes, plus the default theme with a Mica backdrop, plus a hidden theme you might find while clicking around in the app.
- Manage your portal folders: This lets you add, modify and toggle secondary portal folders to be displayed in your portal files, both in normal and in simple mode.
- Pick the pinned row behavior: Clicking on this expander will give you four options to choose between, regarding the top row with pinned files. More on these later.
- Revoke Folder Access: Lets you disconnect the app from the folder you are currently viewing, whether it is the portal folder or the pinned folder. This requires a confirmation, at which point you are given the chance to copy the primary portal and pinned folder's path. Disconnecting the folders will result in the app creating a new window, guiding you through the out of box experience again to set up new folders.

Here are the four settings regarding the pinned row:
- Always opened: This will always start DropStack with the pinned row being expanded. This is the default setting
- Remember last state: This will automatically expand the pinned row if it was expanded when you last closed the app.
- Always close: This will always launch DropStack with the bar being collapsed.
- Protect through Windows Hello™️: This will protect your pinned files and only let you see them after you confirmed it is you via facial recognition, scanning your fingerprint, entering your pin or any other way that is supported by Windows Hello. This is entirely handled by Windows so DropStack never gets to see any of these features. To learn mode, please refer to the [privacy statement](https://github.com/Blindside-Studios/DropStack/main/README.md#privacy-statement) below.

## Privacy statement
### Local abilities
DropStack will only ever access the folder paths that you specifically choose with the folder picker. 
DropStack will also only see top-level files, meaning that anything that is inside a folder or a compressed file cannot be seen by the app. 
DropStack never shares any information about your files with anyone, including first- and third-party software, as well as the developer.
In fact, DropStack's capability to access the internet has been fully disabled.

### About Windows Hello™️
Authenticating with Windows Hello means that only Windows ever gets to see your biometric details, PIN or hardware key. 
There is no point in time where the app is able to see your login credentials or authentication details.

## Update policy
We plan to frequently update DropStack with new features and bug fixes, however, we do not provide any legal binding that entitles you to updates.

## Feedback
If you would like to provide feedback, feel free to do so. We are always happy to listen to your feature suggestions and other things you would like to see changed. If you want to report a bug or suggest a feature, please issue a GitHub ticket. We also plan to add an integrated feedback system into the app down the line.
