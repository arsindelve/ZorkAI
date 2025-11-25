import React, {useState, useEffect} from "react";
import {Button, Menu, MenuItem, ListItemIcon, ListItemText, Badge} from "@mui/material";
import {Mixpanel} from "../services/Mixpanel";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import InventoryIcon from '@mui/icons-material/Inventory';
import BackpackIcon from '@mui/icons-material/Backpack';
import DescriptionIcon from '@mui/icons-material/Description';
import LightbulbIcon from '@mui/icons-material/Lightbulb';
import SportsEsportsIcon from '@mui/icons-material/SportsEsports';

type InventoryButtonProps = {
    onInventoryClick: (verb: string) => void; // Callback prop to send the clicked inventory item to the parent
    inventory: string[]
};

// Function to get an appropriate icon for an inventory item
const getItemIcon = (item: string) => {
    const itemLower = item.toLowerCase();
    if (itemLower.includes('leaflet') || itemLower.includes('letter') || itemLower.includes('note') || itemLower.includes('book')) {
        return <DescriptionIcon fontSize="small" />;
    } else if (itemLower.includes('lantern') || itemLower.includes('lamp') || itemLower.includes('light')) {
        return <LightbulbIcon fontSize="small" />;
    } else if (itemLower.includes('sword') || itemLower.includes('knife') || itemLower.includes('weapon')) {
        return <SportsEsportsIcon fontSize="small" />;
    } else {
        return <BackpackIcon fontSize="small" />;
    }
};

export default function InventoryButton({inventory, onInventoryClick}: InventoryButtonProps) {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

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
            <Badge
                badgeContent={inventory.length}
                color="secondary"
                sx={{
                    '& .MuiBadge-badge': {
                        backgroundColor: '#f44336',
                        color: 'white',
                        fontWeight: 'bold',
                        transition: 'all 0.3s ease',
                    }
                }}
            >
                <Button
                    data-testid="inventory-button"
                    onClick={handleClick}
                    variant="contained"
                    color="primary"
                    startIcon={<InventoryIcon />}
                    endIcon={<KeyboardArrowDownIcon />}
                    disabled={!isLoaded}
                    sx={{
                        borderRadius: '20px',
                        backgroundColor: 'rgba(255, 255, 255, 0.1)',
                        color: 'white',
                        '&:hover': {
                            backgroundColor: 'rgba(255, 255, 255, 0.2)',
                        },
                        transition: 'all 0.3s ease',
                        textTransform: 'none',
                        fontWeight: 'bold',
                        display: { xs: isLoaded ? 'inline-flex' : 'none', sm: 'inline-flex' },
                        opacity: { xs: 1, sm: isLoaded ? 1 : 0.6 },
                        transform: isLoaded ? 'translateY(0)' : 'translateY(10px)',
                    }}
                >
                    Inventory
                </Button>
            </Badge>

            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={() => handleClose()} // Handle menu close
                anchorOrigin={{
                    vertical: "bottom",
                    horizontal: "center",
                }}
                transformOrigin={{
                    vertical: "top",
                    horizontal: "center",
                }}
                slotProps={{
                    paper: {
                        elevation: 3,
                        sx: {
                            borderRadius: '12px',
                            mt: 1,
                            maxHeight: '70vh',
                            '& .MuiMenuItem-root': {
                                px: 2,
                                py: 1.5,
                                transition: 'background-color 0.2s',
                                '&:hover': {
                                    backgroundColor: 'rgba(0, 0, 0, 0.04)',
                                },
                            },
                        }
                    }
                }}
            >
                {/* Map through the inventory array to create MenuItems */}
                {inventory.map((item, index) => (
                    <MenuItem key={index} onClick={() => handleClose(item)}>
                        <ListItemIcon>
                            {getItemIcon(item)}
                        </ListItemIcon>
                        <ListItemText>{item}</ListItemText>
                    </MenuItem>
                ))}
            </Menu>
        </>
    );
}
