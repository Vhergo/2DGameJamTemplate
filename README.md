# 2DGameJamTemplate

Comes with the following basics:

- Scene Manager with two scenes:
    - Main Menu Scene
        - Has a simple adjustable scrolling background
    - Game Scene
    - Simple loading screen available
      
- Persistent Sound Manager:
    - Handles both music and sound effects
    - Allows sound effects to be triggered from anywhere
    - Audio Settings available in both Main Menu and Game scenes
        - Audio sliders
        - Audio toggles
     
- Cinemachine already imported:
    - Simple camera shake script triggerable from anywhere

- 2D Grid Placer Tool
    - Access by going to Tools > 2D Grid Placer
    - Add different groups for every different type of tile prefab you would like to place
    - Specify the parent object and tile prefab (tile must have a collider)
    - Specify the grid size (this is a just a visual indicator to help you place tiles correctly)
    - Adjust toggle buttons as needed and place tiles to create your level
    - You can save your grid to Prefabs or to Files using the respective buttons


- 2D Platformer Player Movement:
    - Assign RigidBody2D and TrailRenderer
    - Create two empty objects (one on the bottom for the ground check and one on the left for the wall check; create and set the repective layers)
    - Adjust default values as you see fit

Coming Soon:
- General reusable scripts (e.g. player movement, animation controller, cursor manager)
- 3D template
