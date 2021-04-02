# The Platformer Kit

The Platformer Kit is a simple implementation of platformer physics, coming with a sandbox aiming to increase productivity developing indie platformers. The kit supports various parameters / events so that developers can easily adopt their ideas into a code.

## Installation

### Installing

You can install The Platformer Kit simply by using Unity Package Manager.

On Unity Menu, 'Window' > 'Package Manager' > Click '+' button > Select 'Add Package from git URL' and enter:

```
https://github.com/JammPark/PlatformerKit.git
```

## Creating a Platformer Character

Find 'Platformer Kit > Platformer Body' on 'Add Component' menu, and you're ready to go! You can write a simple controller 

You can check out the detailed information on the wiki.

### Simple Controller example

```
using UnityEngine;
using JaeminPark.PlatformerKit;

[RequireComponent(typeof(PlatformerBody))]
public class Player : MonoBehaviour
{
    // acceleration per frame when player walks
    public float walk = 0.03f;
    // horizontal drag per frame when player walks
    public float walkDrag = 1.3f;
    // velocity when player jumps
    public float jump = 0.35f;
    // rate of velocity that'll remain when jump button is released
    public float jumpCut = 0.5f;

    // Variables where the input will be stored.
    int horizontal;
    bool jumpDown;
    bool jumpUp;

    // PlatformerBody component
    PlatformerBody body;

    private void Awake()
    {
        body = GetComponent<PlatformerBody>();
    }

    private void Update()
    {
        // We'll update the input variables on Update.
        
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");

        if (horizontalRaw < 0)
            horizontal = -1;
        else if (horizontalRaw > 0)
            horizontal = 1;
        else
            horizontal = 0;

        if (Input.GetButtonDown("Jump"))
            jumpDown = true;

        if (Input.GetButtonUp("Jump"))
            jumpUp = true;
    }

    private void FixedUpdate()
    {
        // Anything related to Physics will be handled on FixedUpdate.
        
        // Walking. you can doodle with walk & walkDrag parameter to get intended control feeling.
        body.velocity.x += horizontal * walk;
        body.velocity.x /= walkDrag;

        // Jumping
        if (jumpDown && body.isGround)
            body.velocity.y = jump;

        // Cutting jump when the button is released. This is cheap short jump implementation!
        if (jumpUp && body.velocity.y > 0)
            body.velocity.y *= (1 - jumpCut);

        jumpDown = false;
        jumpUp = false;
    }
}
```

## License

This project is licensed under the MIT License - see the [license.md](LICENSE.md) file for details.

## Acknowledgments

The project's heavily inspired by @Creta5164's personal implementation of platformer physics for his game. Special thanks to him!