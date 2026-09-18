import React, {useState} from 'react';
import {Button, ListItemIcon, ListItemText, Menu, MenuItem} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import DialogType from '../DialogType';
import {useGameContext} from '../context/GameContext';
import {Mixpanel} from '../utils/Mixpanel';

export type AboutMenuItem = {
    label: string;
    icon: React.ReactNode;
    trackingName: string;
    url?: string;
    dialog?: DialogType;
};

export interface AboutMenuProps {
    latestVersion: string;
    fallbackVersion: string;
    items: AboutMenuItem[];
}

export default function AboutMenu({latestVersion, fallbackVersion, items}: AboutMenuProps) {
    const {setDialogToOpen} = useGameContext();
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
    const open = Boolean(anchorEl);
    const close = () => setAnchorEl(null);

    const activate = (item: AboutMenuItem) => {
        if (item.dialog !== undefined) setDialogToOpen(item.dialog);
        if (item.url) {
            Mixpanel.track('Click on Menu Item', {url: item.url, name: item.trackingName});
            window.open(item.url, '_blank');
        }
        close();
    };

    return (
        <div>
            <Button
                id="about-menu-button"
                data-testid="about-button"
                aria-controls={open ? 'about-menu' : undefined}
                aria-haspopup="true"
                aria-expanded={open ? 'true' : undefined}
                onClick={(event) => setAnchorEl(event.currentTarget)}
                variant="contained"
                color="primary"
                startIcon={<InfoIcon />}
                endIcon={<KeyboardArrowDownIcon />}
                sx={{
                    borderRadius: '20px',
                    backgroundColor: 'rgba(255,255,255,.1)',
                    color: 'white',
                    '&:hover': {backgroundColor: 'rgba(255,255,255,.2)'},
                    transition: 'all .3s ease',
                    textTransform: 'none',
                    fontWeight: 'bold',
                }}
            >
                About
            </Button>
            <Menu
                id="about-menu"
                anchorEl={anchorEl}
                open={open}
                onClose={close}
                MenuListProps={{'aria-labelledby': 'about-menu-button'}}
                slotProps={{
                    paper: {
                        elevation: 3,
                        sx: {
                            borderRadius: '12px',
                            mt: 1,
                            '& .MuiMenuItem-root': {
                                px: 2,
                                py: 1.5,
                                transition: 'background-color .2s',
                                '&:hover': {backgroundColor: 'rgba(0,0,0,.04)'},
                            },
                        },
                    },
                }}
                transformOrigin={{horizontal: 'center', vertical: 'top'}}
                anchorOrigin={{horizontal: 'center', vertical: 'bottom'}}
            >
                {items.map((item) => (
                    <MenuItem
                        key={`${item.trackingName}-${item.label}`}
                        onClick={() => activate(item)}
                    >
                        <ListItemIcon>{item.icon}</ListItemIcon>
                        <ListItemText>{item.label}</ListItemText>
                    </MenuItem>
                ))}
                <MenuItem
                    onClick={() =>
                        activate({
                            label: '',
                            icon: null,
                            trackingName: 'Release Notes',
                            dialog: DialogType.ReleaseNotes,
                        })
                    }
                >
                    <ListItemIcon>
                        <NewReleasesIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Version {latestVersion || fallbackVersion}</ListItemText>
                </MenuItem>
            </Menu>
        </div>
    );
}
