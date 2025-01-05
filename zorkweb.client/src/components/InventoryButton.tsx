import React, {useState} from "react";
import {Button, Menu, MenuItem} from "@mui/material";
import {Mixpanel} from "../Mixpanel.ts";


type InventoryButtonProps = {
    onInventoryClick: (verb: string) => void; // Callback prop to send the clicked inventory item to the parent
    inventory: string[]
};

export default function InventoryButton({inventory, onInventoryClick}: InventoryButtonProps) {

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget); // Set the anchor element
    };

    const handleClose = (item?: string) => {
        if (item) {
            Mixpanel.track('Click Item', {
                "item": item
            });
            onInventoryClick(item); // Pass the clicked item to the parent
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
                Inventory
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
                {inventory.map((item, index) => (
                    <MenuItem key={index} onClick={() => handleClose(item)}>
                        {item}
                    </MenuItem>
                ))}
            </Menu>
        </>
    );

}