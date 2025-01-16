# **Unused Prefab Checker**

This tool is an editor window that helps you find and clean up unused prefabs in your Unity project. You can check whether your prefabs are really used in the project and get rid of unnecessary files. Additionally, it provides logs showing where each prefab is used in the scenes.

---

## **Installation**

### 1. Add the Script to Your Project
To add the script to your project, place the UnusedPrefabChecker.cs file inside the Editor folder of your Unity project. If the Editor folder does not exist, create a new Editor folder inside the Assets directory and then add the script there.

### 2. Open the Editor Window
To open the editor window, go to `Tools > Unused Prefab Checker` in the Unity menu.

---

## **Usage**

### **Adding Prefabs**
To check specific prefabs, add them in the "Selected Prefabs" section. Click the "Add New Prefab" button to add more prefabs.

### **Folder Selection**
If you want to load prefabs from specific folders, you can select folders in the "Selected Folders" section. Use the "Load Prefabs from Folders" button to load prefabs from those folders.

![Loading prefabs from specific folders](https://s3.ezgif.com/tmp/ezgif-3-97701379f5.gif)

### **Scene Selection**
You can choose specific scenes to check for prefab usage in the "Selected Scenes" section. Click the "Add New Scene" button to add scenes.

![Selecting scenes for prefab check](https://s3.ezgif.com/tmp/ezgif-3-16316accb8.gif)

### **Check Operation**
Click the "Check Prefabs" button to start checking whether the selected prefabs are used in the project. This process will analyze prefab usage in the scenes and references between prefabs.

![Checking prefab usage](https://s3.ezgif.com/tmp/ezgif-3-03ebe62165.gif)

### **Results and Log**
Unused prefabs will be listed in the "Unused Prefabs" section. During the check, the log will display where each prefab is used (which scenes or which references), helping you track the usage of prefabs in your project.

![Viewing prefab usage results](https://s3.ezgif.com/tmp/ezgif-3-e9082499fb.gif)

### **Export Results**
You can export a report of unused prefabs by clicking the "Export Results" button. The report will be saved as a `.txt` file containing the names of unused prefabs.

### **Clear All**
You can remove all selected prefabs, folders, and scenes by clicking the "Clear All" button.

---

## **Features**

- **Prefab Check:** Checks whether selected prefabs are used in the project.
- **Folder Load:** Allows you to load prefabs from specific folders.
- **Scene Check:** Lets you check usage in selected scenes.
- **Log Information:** Shows where unused prefabs are used (in which scenes or with which references) via log.
- **Filtering:** Easily filter out unused prefabs.
- **Report Generation:** Generate a detailed report of unused prefabs.
- **Simple Interface:** Easily select prefabs, folders, and scenes.

---

## **Notes**

- This tool only checks prefabs and scriptable objects. It can also check prefab references inside scripts.
- Analyzing references between prefabs may take some time, especially in larger projects, so expect longer processing times.

---

## **Contact & Feedback**

If you find any issues, have suggestions, or think something is missing, feel free to reach out!

- **Email:** [ayberkturksoy97@gmail.com](mailto:ayberkturksoy97@gmail.com)
- **LinkedIn:** [https://www.linkedin.com/in/ayberkturksoy/](https://www.linkedin.com/in/ayberkturksoy/)

