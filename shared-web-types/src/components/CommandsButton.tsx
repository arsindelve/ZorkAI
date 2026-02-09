import React, {useState, useEffect} from "react";
import {Button, Menu, MenuItem, ListItemText, Box} from "@mui/material";
import {Mixpanel} from "../utils/Mixpanel";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import TerminalIcon from '@mui/icons-material/Terminal';

type CommandButtonProps = {
    onCommandClick: (command: string) => void; // Callback prop to send the clicked command to the parent
};

const toSentenceCase = (s: string) => s.charAt(0).toUpperCase() + s.slice(1);

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
                <Box component="span" sx={{ display: { xs: 'none', sm: 'inline' } }}>Commands</Box>
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
                {commands.map((command, index) => (
                    <MenuItem key={index} onClick={() => handleClose(command)}>
                        <ListItemText>{toSentenceCase(command)}</ListItemText>
                    </MenuItem>
                ))}
            </Menu>
        </>
    );
}
