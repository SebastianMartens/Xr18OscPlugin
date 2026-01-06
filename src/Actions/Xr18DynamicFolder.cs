// #nullable disable

// namespace Loupedeck.Xr18OscPlugin.Actions;

// using System.Collections.Generic;

// /// <summary>
// /// No useful content yet. Only for experiementing with dynamic folders.
// /// </summary>
// public class Xr18DynamicFolder : PluginDynamicFolder
// {     
//     public Xr18DynamicFolder()
//     {
//         DisplayName = "Dynamic Folder by Bart";
//         GroupName = "Barts Plugin";            
//     }

//     public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _) => base.GetNavigationArea(_);

//     public override IEnumerable<string> GetButtonPressActionNames(DeviceType deviceType)
//     {
//        return new[] {
//             PluginDynamicFolder.NavigateUpActionName,
//             CreateCommandName("7"),
//             CreateCommandName("8"),
//             CreateCommandName("9"),
//             CreateCommandName("."),
//             CreateCommandName("4"),
//             CreateCommandName("5"),
//             CreateCommandName("6"),
//             CreateCommandName("0"),
//             CreateCommandName("1"),
//             CreateCommandName("2"),
//             CreateCommandName("3")
//         };
//     }

//     public override bool Activate() => base.Activate();
// }
