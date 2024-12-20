import React, { useState } from "react";
import { Button, Menu, MenuItem } from "@mui/material";
import {Mixpanel} from "./Mixpanel.ts";

type CommandButtonProps = {
    onCommandClick: (command: string) => void; // Callback prop to send the clicked command to the parent
};

export default function CommandsButton({ onCommandClick }: CommandButtonProps) {
    const commands = ["enter", "exit", "go up", "go down", "wait", "inventory", "take all", "look", "again"];
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget); // Set the anchor element
    };

    const handleClose = (command?: string) => {
        if (command) {
            Mixpanel.track('Click Command', {
                "command": command
            });
            onCommandClick(command); // Pass the clicked command to the parent
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
                Commands
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
                {/* Map through the commands array to create MenuItems */}
                {commands.map((command, index) => (
                    <MenuItem key={index} onClick={() => handleClose(command)}>
                        {command}
                    </MenuItem>
                ))}
            </Menu>
        </>
    );
}