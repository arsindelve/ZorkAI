import React, {useState} from "react";
import {Button, Menu, MenuItem} from "@mui/material";
import {Mixpanel} from "./Mixpanel.ts";

type VerbsButtonProps = {
    onVerbClick: (verb: string) => void; // Callback prop to send the clicked verb to the parent
};

export default function VerbsButton({onVerbClick}: VerbsButtonProps) {
    const verbs = ["eat", "drink", "drop", "attack", "turn on", "turn off", "read", "open", "close", "examine", "take"];
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget); // Set the anchor element
    };

    const handleClose = (verb?: string) => {
        if (verb) {
            Mixpanel.track('Click Verb', {
                "verb": verb
            });
            onVerbClick(verb); // Pass the clicked verb to the parent
        }
        setAnchorEl(null); // Close the menu
    };

    return (
        <>
            <Button
                onClick={handleClick}
                sx={{
                    color: "white",          // Set text color to white
                    fontFamily: "inherit",   // Use the default font
                    textTransform: "none",   // Prevent text from being all-caps
                }}
            >
                Verbs
            </Button>

            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={() => handleClose()} // Handle menu close
                anchorOrigin={{
                    vertical: "top",
                    horizontal: "left",
                }}
                transformOrigin={{
                    vertical: "bottom",
                    horizontal: "left",
                }}
            >
                {/* Map through the verbs array to create MenuItems */}
                {verbs.map((verb, index) => (
                    <MenuItem className={"uppercase"} key={index} onClick={() => handleClose(verb)}>
                        {verb}
                    </MenuItem>
                ))}
            </Menu>
        </>
    );
}