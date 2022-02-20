# NOTICE : This repo is deprecated, the same addition is included in [here.](https://github.com/b3x206/bxfw-unity)

# Unity Tab System
[![CodeFactor](https://www.codefactor.io/repository/github/b3x206/unity-tabsystem/badge)](https://www.codefactor.io/repository/github/b3x206/unity-tabsystem)
Use this tab system to easily create tabs in your UI.

## Notes :
Before usage, make sure to import TextMeshPro text.
![How To Import](img/import-tmp.jpg?raw=true, "Import TMP")

## Preview :
![Scene](img/preview.jpg?raw=true "How it look (with customize)")
![Scene](img/previewAnim.gif?raw=true "Toggling menus")

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
    // Sets tab button amount
    tabSystem.TabButtonAmount = 4;
}

// Register this event from inspector.
public void EventExample(int SelectedTabIndex)
{
    // Information
    Debug.Log(string.Format("Selected tab was : {0}", SelectedTabIndex));
    // Toggle menus from here.
}
```

## Documentation :
What's that? (how do i even write documentation anyway)
* Create tab using GameObject>UI>TabSystem
* Just play with the 'TabAmount' and use the
'CurrentReferenceTabIndex' variable for which tab to instantiate from.

## FAQ :
* Q: I found a bug!
* A: Please open issue (with enough reproduction detail) thank

* Q: I made your bad code better
* A: Ok open merge request i will (probably not look) definetly look
