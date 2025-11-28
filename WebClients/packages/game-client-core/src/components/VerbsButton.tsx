import React, {useState, useEffect} from "react";
import {Button, Menu, MenuItem, ListItemIcon, ListItemText} from "@mui/material";
import {
    KeyboardArrowDown as KeyboardArrowDownIcon,
    TextFormat as TextFormatIcon,
    Restaurant as RestaurantIcon,
    LocalDrink as LocalDrinkIcon,
    DeleteOutline as DeleteOutlineIcon,
    SportsKabaddi as SportsKabaddiIcon,
    PowerSettingsNew as PowerSettingsNewIcon,
    PowerOff as PowerOffIcon,
    MenuBook as MenuBookIcon,
    LockOpen as LockOpenIcon,
    Lock as LockIcon,
    Search as SearchIcon,
    PanTool as PanToolIcon
} from '@mui/icons-material';
import {Mixpanel} from "../services/Mixpanel";

type VerbsButtonProps = {
    onVerbClick: (verb: string) => void; // Callback prop to send the clicked verb to the parent
};

// Map verbs to their corresponding icon components
const verbIcons: Record<string, any> = {
    "eat": RestaurantIcon,
    "drink": LocalDrinkIcon,
    "drop": DeleteOutlineIcon,
    "attack": SportsKabaddiIcon,
    "turn on": PowerSettingsNewIcon,
    "turn off": PowerOffIcon,
    "read": MenuBookIcon,
    "open": LockOpenIcon,
    "close": LockIcon,
    "examine": SearchIcon,
    "take": PanToolIcon
};

export default function VerbsButton({onVerbClick}: VerbsButtonProps) {
    const verbs = ["eat", "drink", "drop", "attack", "turn on", "turn off", "read", "open", "close", "examine", "take"];
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

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
                variant="contained"
                color="primary"
                startIcon={<TextFormatIcon />}
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
                Verbs
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
                {/* Map through the verbs array to create MenuItems */}
                {verbs.map((verb, index) => {
                    const IconComponent = verbIcons[verb] || TextFormatIcon;
                    return (
                        <MenuItem key={index} onClick={() => handleClose(verb)}>
                            <ListItemIcon>
                                <IconComponent fontSize="small" />
                            </ListItemIcon>
                            <ListItemText>{verb}</ListItemText>
                        </MenuItem>
                    );
                })}
            </Menu>
        </>
    );
}
