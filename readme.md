# Unity Tab System
[![CodeFactor](https://www.codefactor.io/repository/github/b3x206/unity-tabsystem/badge)](https://www.codefactor.io/repository/github/b3x206/unity-tabsystem)

Use this tab system to easily create tabs in your UI.

# Notice :
I have reopened this repository because [bxfw-unity](https://github.com/b3x206/bxfw-unity) is just a mess.

This will only get minor updates, until it's perfect.

If there's any problem, just open an issue.

----------
## Notes :
Before usage, make sure to import TextMeshPro text.


![How To Import](img/import-tmp.jpg?raw=true, "Import TMP")

## Preview :
![Scene](img/preview.png?raw=true "How it look (with customize)")
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
What's that? (how do i even write documentation anyway)
* Create tab using GameObject>UI>TabSystem
* Just play with the 'TabAmount' and use the 'CurrentReferenceTabIndex' variable for which tab to instantiate from.

## After install / download :
* You can delete the tempoary images folder.
