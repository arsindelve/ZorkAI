import React, {useState, useEffect} from "react";
import {Button, Menu, MenuItem, ListItemText, Box} from "@mui/material";
import {Mixpanel} from "../utils/Mixpanel";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import TextFormatIcon from '@mui/icons-material/TextFormat';

type VerbsButtonProps = {
    onVerbClick: (verb: string) => void; // Callback prop to send the clicked verb to the parent
};

const toSentenceCase = (s: string) => s.charAt(0).toUpperCase() + s.slice(1);

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
                    borderRadius: { xs: '50%', sm: '20px' },
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
                    minWidth: { xs: 'auto', sm: '64px' },
                    px: { xs: 1.5, sm: 2 },
                    py: { xs: 1, sm: undefined },
                    '& .MuiButton-endIcon': { display: { xs: 'none', sm: 'flex' } },
                    '& .MuiButton-startIcon': { mr: { xs: 0, sm: 1 } },
                }}
            >
                <Box component="span" sx={{ display: { xs: 'none', sm: 'inline' } }}>Verbs</Box>
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
                        <ListItemText>{toSentenceCase(verb)}</ListItemText>
                    </MenuItem>
                ))}
            </Menu>
        </>
    );
}
