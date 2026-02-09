import React, {useState, useEffect, useRef} from "react";
import {Button, Menu, MenuItem, ListItemText, Badge, Popper, Paper, MenuList, ClickAwayListener, Grow, Box} from "@mui/material";
import {Mixpanel} from "../utils/Mixpanel";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import InventoryIcon from '@mui/icons-material/Inventory';

type InventoryButtonProps = {
    onInventoryClick: (item: string) => void;
    onActionClick: (action: string) => void;
    inventory: string[];
    inventoryActions: Record<string, string[]>;
};

const toSentenceCase = (s: string) => s.charAt(0).toUpperCase() + s.slice(1);

export default function InventoryButton({inventory, inventoryActions, onInventoryClick, onActionClick}: InventoryButtonProps) {
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

        const actions = inventoryActions[item];
        if (actions && actions.length > 0) {
            setSubmenuAnchorEl(event.currentTarget);
            setActiveItem(item);
        } else {
            setSubmenuAnchorEl(null);
            setActiveItem(null);
        }
    };

    const handleItemClick = (item: string) => {
        Mixpanel.track('Click Item', { "item": item });
        onInventoryClick(item);
        handleClose();
    };

    const handleActionClick = (action: string) => {
        Mixpanel.track('Click Inventory Action', { "action": action });
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

    // Use inventoryActions keys if available, otherwise fall back to inventory array
    const items = Object.keys(inventoryActions).length > 0
        ? Object.keys(inventoryActions)
        : inventory;

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
                    data-testid="inventory-button"
                    onClick={handleClick}
                    variant="contained"
                    color="primary"
                    startIcon={<InventoryIcon />}
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
                    <Box component="span" sx={{ display: { xs: 'none', sm: 'inline' } }}>Inventory</Box>
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
                    const hasActions = inventoryActions[item] && inventoryActions[item].length > 0;
                    return (
                        <MenuItem
                            key={index}
                            onClick={() => handleItemClick(item)}
                            onMouseEnter={(e) => handleItemHover(e, item)}
                            sx={{
                                backgroundColor: activeItem === item ? 'rgba(0, 0, 0, 0.08)' : 'transparent',
                            }}
                        >
                            <ListItemText>{toSentenceCase(item)}</ListItemText>
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
                                    {activeItem && inventoryActions[activeItem]?.map((action, index) => (
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
                                            <ListItemText>{toSentenceCase(action)}</ListItemText>
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
