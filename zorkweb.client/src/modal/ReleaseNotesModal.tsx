import React from "react";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Button from "@mui/material/Button";
import { Typography, Paper, Box, Divider, Skeleton } from "@mui/material";
import UpdateIcon from '@mui/icons-material/Update';

/**
 * Decodes HTML entities in the release notes.
 */
const decodeHTML = (html: string) => {
    const txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
};

const ReleaseNotesModal: React.FC<{
    open: boolean;
    handleClose: () => void;
    releases: { date: string; name: string; notes: string }[];
}> = ({ open, handleClose, releases }) => {

    // Function to render loading skeletons
    const renderSkeletons = () => {
        return Array(3).fill(0).map((_, index) => (
            <Paper
                key={`skeleton-${index}`}
                elevation={2}
                sx={{
                    p: 3,
                    mb: 3,
                    borderRadius: '8px',
                    bgcolor: 'background.paper',
                }}
            >
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <Skeleton variant="circular" width={24} height={24} sx={{ mr: 1 }} />
                    <Skeleton variant="text" width={200} height={32} />
                    <Skeleton variant="text" width={120} height={24} sx={{ ml: 2 }} />
                </Box>

                <Skeleton variant="text" width="100%" height={20} />
                <Skeleton variant="text" width="95%" height={20} />
                <Skeleton variant="text" width="90%" height={20} />
                <Skeleton variant="text" width="97%" height={20} />
                <Skeleton variant="text" width="85%" height={20} />

                <Box sx={{ mt: 2 }}>
                    <Skeleton variant="text" width="92%" height={20} />
                    <Skeleton variant="text" width="88%" height={20} />
                    <Skeleton variant="text" width="94%" height={20} />
                </Box>
            </Paper>
        ));
    };

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            aria-labelledby="release-notes-title"
            maxWidth="md"
            fullWidth
            PaperProps={{
                style: { 
                    borderRadius: '12px',
                    overflow: 'hidden',
                    maxHeight: '90vh'
                },
            }}
        >
            <DialogTitle 
                id="release-notes-title"
                className="bg-gradient-to-r from-gray-800 to-gray-900"
                sx={{
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2
                }}
            >
                <UpdateIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    Zork AI Release Notes
                </Typography>
            </DialogTitle>

            <DialogContent sx={{ p: 3, pb: 1 }}>
                {releases.length === 0 ? (
                    <Box sx={{ pt: 2, pb: 2 }}>
                        {renderSkeletons()}
                    </Box>
                ) : (
                    <Box sx={{ pt: 1 }}>
                        {releases.map((release) => (
                            <Paper
                                key={release.date}
                                elevation={2}
                                sx={{
                                    p: 3,
                                    mb: 3,
                                    borderRadius: '8px',
                                    bgcolor: 'background.paper',
                                    transition: 'all 0.2s',
                                    '&:hover': {
                                        transform: 'translateY(-2px)',
                                        boxShadow: 4
                                    }
                                }}
                            >
                                <Typography 
                                    variant="h6" 
                                    fontWeight="bold" 
                                    sx={{ 
                                        mb: 1, 
                                        color: 'grey.800',
                                        display: 'flex',
                                        alignItems: 'center'
                                    }}
                                >
                                    {release.name}
                                    <Typography 
                                        variant="subtitle1" 
                                        component="span" 
                                        sx={{ 
                                            ml: 2,
                                            color: 'text.secondary',
                                            fontWeight: 'normal'
                                        }}
                                    >
                                        {new Date(release.date).toLocaleDateString()}
                                    </Typography>
                                </Typography>

                                <Divider sx={{ mb: 2 }} />

                                <Box
                                    dangerouslySetInnerHTML={{__html: decodeHTML(release.notes)}}
                                    sx={{
                                        whiteSpace: "normal",
                                        wordWrap: "break-word",
                                        color: 'grey.800',
                                        fontSize: '1rem',
                                        lineHeight: 1.6,
                                        '& ul': {
                                            pl: 4,
                                            mb: 2,
                                            listStyleType: 'disc',
                                            listStylePosition: 'outside'
                                        },
                                        '& ol': {
                                            pl: 4,
                                            mb: 2,
                                            listStyleType: 'decimal',
                                            listStylePosition: 'outside'
                                        },
                                        '& li': {
                                            mb: 0.5,
                                            ml: 0,
                                            display: 'list-item'
                                        },
                                        '& h2': {
                                            fontSize: '1.25rem',
                                            fontWeight: 'bold',
                                            mt: 2,
                                            mb: 1
                                        },
                                        '& h3': {
                                            fontSize: '1.1rem',
                                            fontWeight: 'bold',
                                            mt: 1.5,
                                            mb: 1
                                        },
                                        '& p': {
                                            mb: 1
                                        },
                                        '& a': {
                                            color: 'primary.main',
                                            textDecoration: 'none',
                                            '&:hover': {
                                                textDecoration: 'underline'
                                            }
                                        }
                                    }}
                                />
                            </Paper>
                        ))}
                    </Box>
                )}
            </DialogContent>

            <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
                <Button 
                    onClick={handleClose} 
                    variant="outlined"
                    sx={{ 
                        borderRadius: '20px',
                        px: 3,
                        borderColor: 'grey.600',
                        color: 'grey.800',
                        '&:hover': {
                            borderColor: 'grey.800',
                            bgcolor: 'grey.100'
                        }
                    }}
                >
                    Close
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ReleaseNotesModal;
