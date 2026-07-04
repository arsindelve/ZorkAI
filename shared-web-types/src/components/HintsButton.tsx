import {useEffect, useState} from "react";
import {Box, Button} from "@mui/material";
import TipsAndUpdatesOutlinedIcon from "@mui/icons-material/TipsAndUpdatesOutlined";
import {Mixpanel} from "../utils/Mixpanel";

type HintsButtonProps = {
    /** Whether the hint panel is currently open — the button lights up while it is. */
    open: boolean;
    onToggle: () => void;
};

/**
 * Toggle for the hint side panel. Lives in the game input-bar button row alongside
 * Verbs/Commands/Inventory, and matches their styling.
 */
export default function HintsButton({open, onToggle}: HintsButtonProps) {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

    const handleClick = () => {
        Mixpanel.track(open ? "Close Hint Panel" : "Open Hint Panel", {});
        onToggle();
    };

    return (
        <Button
            onClick={handleClick}
            variant="contained"
            color="primary"
            startIcon={<TipsAndUpdatesOutlinedIcon/>}
            disabled={!isLoaded}
            data-testid="hints-button"
            sx={{
                borderRadius: {xs: "50%", sm: "20px"},
                backgroundColor: open
                    ? "color-mix(in srgb, var(--hint-accent, #f59e0b) 30%, transparent)"
                    : "rgba(255, 255, 255, 0.1)",
                color: "white",
                "&:hover": {
                    backgroundColor: open
                        ? "color-mix(in srgb, var(--hint-accent, #f59e0b) 40%, transparent)"
                        : "rgba(255, 255, 255, 0.2)"
                },
                transition: "all 0.3s ease",
                textTransform: "none",
                fontWeight: "bold",
                display: {xs: isLoaded ? "inline-flex" : "none", sm: "inline-flex"},
                opacity: {xs: 1, sm: isLoaded ? 1 : 0.6},
                transform: isLoaded ? "translateY(0)" : "translateY(10px)",
                minWidth: {xs: "auto", sm: "64px"},
                px: {xs: 1.5, sm: 2},
                py: {xs: 1, sm: undefined},
                "& .MuiButton-startIcon": {mr: {xs: 0, sm: 1}}
            }}
        >
            <Box component="span" sx={{display: {xs: "none", sm: "inline"}}}>Hints</Box>
        </Button>
    );
}
