import React, {useState, useEffect} from "react";
import {Button, Menu, MenuItem, ListItemIcon, ListItemText} from "@mui/material";
import {Mixpanel} from "../services/Mixpanel";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import TextFormatIcon from '@mui/icons-material/TextFormat';
import RestaurantIcon from '@mui/icons-material/Restaurant';
import LocalDrinkIcon from '@mui/icons-material/LocalDrink';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import SportsKabaddiIcon from '@mui/icons-material/SportsKabaddi';
import PowerSettingsNewIcon from '@mui/icons-material/PowerSettingsNew';
import PowerOffIcon from '@mui/icons-material/PowerOff';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import LockOpenIcon from '@mui/icons-material/LockOpen';
import LockIcon from '@mui/icons-material/Lock';
import SearchIcon from '@mui/icons-material/Search';
import PanToolIcon from '@mui/icons-material/PanTool';

type VerbsButtonProps = {
    onVerbClick: (verb: string) => void; // Callback prop to send the clicked verb to the parent
};

// Map verbs to their corresponding icons
const verbIcons: Record<string, React.ReactElement> = {
    "eat": <RestaurantIcon fontSize="small" />,
    "drink": <LocalDrinkIcon fontSize="small" />,
    "drop": <DeleteOutlineIcon fontSize="small" />,
    "attack": <SportsKabaddiIcon fontSize="small" />,
    "turn on": <PowerSettingsNewIcon fontSize="small" />,
    "turn off": <PowerOffIcon fontSize="small" />,
    "read": <MenuBookIcon fontSize="small" />,
    "open": <LockOpenIcon fontSize="small" />,
    "close": <LockIcon fontSize="small" />,
    "examine": <SearchIcon fontSize="small" />,
    "take": <PanToolIcon fontSize="small" />
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
                {verbs.map((verb, index) => (
                    <MenuItem key={index} onClick={() => handleClose(verb)}>
                        <ListItemIcon>
                            {verbIcons[verb] || <TextFormatIcon fontSize="small" />}
                        </ListItemIcon>
                        <ListItemText>{verb}</ListItemText>
                    </MenuItem>
                ))}
            </Menu>
        </>
    );
}
