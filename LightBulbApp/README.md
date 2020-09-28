#LightBulbApp
App that handles connection between a bluetooth keyboard and a Yeelight Bulb via Raspberry PI's bluetooth.



Set this script to run at startup.
```ShellSession
crontab -e
@restart /path/to/this/script/LightBulbApp
```


Remap the Code enum to match your preffered bindings.
Current bindings are set to number 1-6 which perfetcly match this keyboard: 

![Keyboard Logo](/images/keyboard.png)
Format: ![Alt Text](url)