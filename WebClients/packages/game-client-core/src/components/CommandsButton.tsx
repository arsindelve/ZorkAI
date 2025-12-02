import React, {useState, useEffect} from "react";
import {Button, Menu, MenuItem, ListItemIcon, ListItemText} from "@mui/material";
import {
    KeyboardArrowDown as KeyboardArrowDownIcon,
    Terminal as TerminalIcon,
    Settings as SettingsIcon,
    HealthAndSafety as HealthAndSafetyIcon,
    Login as LoginIcon,
    Logout as LogoutIcon,
    KeyboardArrowUp as KeyboardArrowUpIcon,
    HourglassEmpty as HourglassEmptyIcon,
    Inventory as InventoryIcon,
    Replay as ReplayIcon,
    DeleteSweep as DeleteSweepIcon,
    AddShoppingCart as AddShoppingCartIcon,
    Visibility as VisibilityIcon
} from '@mui/icons-material';
import {Mixpanel} from "../services/Mixpanel";

type CommandButtonProps = {
    onCommandClick: (command: string) => void; // Callback prop to send the clicked command to the parent
};

// Map commands to their corresponding icon components
const commandIcons: Record<string, any> = {
    "verbose": SettingsIcon,
    "diagnose": HealthAndSafetyIcon,
    "enter": LoginIcon,
    "exit": LogoutIcon,
    "go up": KeyboardArrowUpIcon,
    "go down": KeyboardArrowDownIcon,
    "wait": HourglassEmptyIcon,
    "inventory": InventoryIcon,
    "again": ReplayIcon,
    "drop all": DeleteSweepIcon,
    "take all": AddShoppingCartIcon,
    "look": VisibilityIcon
};

export default function CommandsButton({onCommandClick}: CommandButtonProps) {
    const commands = ["verbose", "diagnose", "enter", "exit", "go up", "go down", "wait", "inventory", "again", "drop all", "take all", "look"];
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

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
                variant="contained"
                color="primary"
                startIcon={<TerminalIcon />}
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
                Commands
            </Button>

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
                {/* Map through the commands array to create MenuItems */}
                {commands.map((command, index) => {
                    const IconComponent = commandIcons[command] || TerminalIcon;
                    return (
                        <MenuItem key={index} onClick={() => handleClose(command)}>
                            <ListItemIcon>
                                <IconComponent fontSize="small" />
                            </ListItemIcon>
                            <ListItemText>{command}</ListItemText>
                        </MenuItem>
                    );
                })}
            </Menu>
        </>
    );
}
