# ClimbingApp
Creators: Benedict Hildisch (bhildisc@ethz.ch), Felix Schnarrenberger (schnife@ethz.ch), David Filiberti (fildavid@ethz.ch), Felix Schätzle (felschae@ethz.ch)

Supervised by Eric Vollenweider and Taein Kwon
#

We developed a Mixed Reality application that improves the overall experience of learning rock-climbing. In a record mode the grasps of an expert are captured once for a specific route. Then, an amateur has holograms of the suggested grasps augmented in their field of view. Grasp detection and iterating through grasp holograms are the main functionalities enabling this method. The resulting application fulfills its goals and was stable in use. Testing showed that this approach was helpful and efficient for most users. In future works, foot tracking and multi device capabilities could be added. (https://www.youtube.com/watch?v=z1qRYEteMUU)
#

## Here are intructions on how to set up the project.
### Download Unity 2021.3.12f1 and MRTK 2.8
#
### Follow the instructions to create an Azure account to use Azure Spatial Anchors
#### https://learn.microsoft.com/en-us/azure/spatial-anchors/how-tos/create-asa-account?tabs=azure-portal
#
### Copy "ProjectSettings", "UserSettings" and the two subfolders of "Assets" ("Scenes" and "clone") into your Unity project
#
### Add the following voice commands in the MRTK settings
#### "start" : start a mode
#### "pause" : pausing a mode
#### "finish" : finishing a mode and returning to mode selection
#### "delete last" : delete last grasp in record mode
#### "save left" : save left grasp in record mode
#### "save right" : save right grasp in record mode
#### "change color" : change color of grasps
