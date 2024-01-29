# Unity Tab System
[![CodeFactor](https://www.codefactor.io/repository/github/b3x206/unity-tabsystem/badge)](https://www.codefactor.io/repository/github/b3x206/unity-tabsystem)

Use this tab system to easily create tabs in your UI. (like the browser tabs or menu tabs)

## Notes :
Before usage, make sure to import TextMeshPro text.


![How To Import](https://raw.githubusercontent.com/b3x206/unity-tabsystem/resources/img/import-tmp.jpg "Import TMP")

## Preview :
![Scene](https://raw.githubusercontent.com/b3x206/unity-tabsystem/resources/img/preview.png "How it look (with customize)")
![Scene](https://raw.githubusercontent.com/b3x206/unity-tabsystem/resources/img/previewAnim.gif "Toggling menus")

[Font Used In The Previews](https://fonts.google.com/specimen/Comfortaa)

## Code :
```C#
// Reference to the tab system.
public TabSystem tabSystem;

void Start()
{
    // Register event
    tabSystem.OnTabButtonsClicked.AddListener(EventExample);
    // Sets current selected tab to index (if the index exists)
    tabSystem.SetSelectedButtonIndex(2);
    // Sets tab button amount (this generates tabs if the count isn't already 4)
    tabSystem.TabButtonAmount = 4;
}

// ...
// Register this event from inspector if you want, using the unity event.
public void EventExample(int SelectedTabIndex)
{
    // Information
    Debug.Log(string.Format("Selected tab was : {0}", SelectedTabIndex));
    // Toggle menus from here.
}
```

## Documentation :
* Documentation will be added as a wiki page, but this is the quick start thing.
* Create tab using GameObject&gt;UI&gt;TabSystem
* Just play with the 'TabAmount' and use the 'CurrentReferenceTabIndex' variable for which tab to instantiate from.
* Use the inspector (like you would do on a GameObject&gt;UI&gt;Button) to attach events and change stuff to your liking.

## Licensing :
This repository is dual licensed to be both [unlicense/public domain](https://unlicense.org) and/or [MIT license](https://opensource.org/license/mit/). Use the one that is suitable for you and your organization.
