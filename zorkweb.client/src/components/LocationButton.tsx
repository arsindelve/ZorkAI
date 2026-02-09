import React, {useState, useEffect, useRef} from "react";
import {Button, Menu, MenuItem, ListItemIcon, ListItemText, Badge, Popper, Paper, MenuList, ClickAwayListener, Grow} from "@mui/material";
import {Mixpanel} from "../Mixpanel.ts";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import PlaceIcon from '@mui/icons-material/Place';
import ChairIcon from '@mui/icons-material/Chair';
import DoorFrontIcon from '@mui/icons-material/DoorFront';
import NatureIcon from '@mui/icons-material/Nature';
import CategoryIcon from '@mui/icons-material/Category';

type LocationButtonProps = {
    onItemClick: (item: string) => void;
    onActionClick: (action: string) => void;
    locationActions: Record<string, string[]>;
};

const getItemIcon = (item: string) => {
    const itemLower = item.toLowerCase();
    if (itemLower.includes('door') || itemLower.includes('gate') || itemLower.includes('window')) {
        return <DoorFrontIcon fontSize="small" />;
    } else if (itemLower.includes('tree') || itemLower.includes('plant') || itemLower.includes('bush') || itemLower.includes('forest')) {
        return <NatureIcon fontSize="small" />;
    } else if (itemLower.includes('chair') || itemLower.includes('table') || itemLower.includes('bed') || itemLower.includes('furniture')) {
        return <ChairIcon fontSize="small" />;
    } else {
        return <CategoryIcon fontSize="small" />;
    }
};

export default function LocationButton({locationActions, onItemClick, onActionClick}: LocationButtonProps) {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const [activeItem, setActiveItem] = useState<string | null>(null);
    const [submenuAnchorEl, setSubmenuAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [isLoaded, setIsLoaded] = useState(false);
    const submenuTimeoutRef = useRef<NodeJS.Timeout | null>(null);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

    // Cleanup timeout on unmount
    useEffect(() => {
        return () => {
            if (submenuTimeoutRef.current) {
                clearTimeout(submenuTimeoutRef.current);
            }
        };
    }, []);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
        setSubmenuAnchorEl(null);
        setActiveItem(null);
        if (submenuTimeoutRef.current) {
            clearTimeout(submenuTimeoutRef.current);
        }
    };

    const handleItemHover = (event: React.MouseEvent<HTMLLIElement>, item: string) => {
        // Clear any pending timeout
        if (submenuTimeoutRef.current) {
            clearTimeout(submenuTimeoutRef.current);
            submenuTimeoutRef.current = null;
        }

        const actions = locationActions[item];
        if (actions && actions.length > 0) {
            setSubmenuAnchorEl(event.currentTarget);
            setActiveItem(item);
        } else {
            setSubmenuAnchorEl(null);
            setActiveItem(null);
        }
    };

    const handleItemClick = (item: string) => {
        Mixpanel.track('Click Location Item', { "item": item });
        onItemClick(item);
        handleClose();
    };

    const handleActionClick = (action: string) => {
        Mixpanel.track('Click Location Action', { "action": action });
        onActionClick(action);
        handleClose();
    };

    const handleSubmenuMouseEnter = () => {
        // Cancel any pending close
        if (submenuTimeoutRef.current) {
            clearTimeout(submenuTimeoutRef.current);
            submenuTimeoutRef.current = null;
        }
    };

    const handleSubmenuMouseLeave = () => {
        // Delay closing the submenu to allow moving back to main menu
        submenuTimeoutRef.current = setTimeout(() => {
            setSubmenuAnchorEl(null);
            setActiveItem(null);
        }, 150);
    };

    // Only show items that have actions
    const items = Object.keys(locationActions).filter(
        item => locationActions[item] && locationActions[item].length > 0
    );

    return (
        <>
            <Badge
                badgeContent={items.length}
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
                    data-testid="location-button"
                    onClick={handleClick}
                    variant="contained"
                    color="primary"
                    startIcon={<PlaceIcon />}
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
                    Location
                </Button>
            </Badge>

            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
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
                {items.map((item, index) => {
                    const hasActions = locationActions[item] && locationActions[item].length > 0;
                    return (
                        <MenuItem
                            key={index}
                            onClick={() => handleItemClick(item)}
                            onMouseEnter={(e) => handleItemHover(e, item)}
                            sx={{
                                backgroundColor: activeItem === item ? 'rgba(0, 0, 0, 0.08)' : 'transparent',
                            }}
                        >
                            <ListItemIcon>
                                {getItemIcon(item)}
                            </ListItemIcon>
                            <ListItemText>{item}</ListItemText>
                            {hasActions && (
                                <ChevronRightIcon fontSize="small" sx={{ ml: 1, color: 'text.secondary' }} />
                            )}
                        </MenuItem>
                    );
                })}
            </Menu>

            {/* Submenu using Popper for better hover behavior */}
            <Popper
                open={Boolean(submenuAnchorEl) && activeItem !== null}
                anchorEl={submenuAnchorEl}
                placement="right-start"
                transition
                style={{ zIndex: 1300 }}
            >
                {({ TransitionProps }) => (
                    <Grow {...TransitionProps}>
                        <Paper
                            elevation={4}
                            sx={{
                                borderRadius: '8px',
                                ml: 0.5,
                            }}
                            onMouseEnter={handleSubmenuMouseEnter}
                            onMouseLeave={handleSubmenuMouseLeave}
                        >
                            <ClickAwayListener onClickAway={handleClose}>
                                <MenuList>
                                    {activeItem && locationActions[activeItem]?.map((action, index) => (
                                        <MenuItem
                                            key={index}
                                            onClick={() => handleActionClick(action)}
                                            sx={{
                                                px: 2,
                                                py: 1,
                                                fontSize: '0.9rem',
                                                '&:hover': {
                                                    backgroundColor: 'rgba(132, 204, 22, 0.15)',
                                                },
                                            }}
                                        >
                                            <ListItemText>{action}</ListItemText>
                                        </MenuItem>
                                    ))}
                                </MenuList>
                            </ClickAwayListener>
                        </Paper>
                    </Grow>
                )}
            </Popper>
        </>
    );
}
