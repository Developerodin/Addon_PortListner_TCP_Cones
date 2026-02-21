Set WshShell = CreateObject("WScript.Shell")
WshShell.CurrentDirectory = "C:\Users\Lenovo\Desktop\Addon\Port_Listener\"
WshShell.Run """C:\Users\Lenovo\Desktop\Addon\Port_Listener\publish\PortListener.exe""", 0, False
