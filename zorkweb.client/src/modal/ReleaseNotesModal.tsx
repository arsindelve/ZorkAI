import React, { useEffect, useState } from "react";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Button from "@mui/material/Button";
import { ReleaseNotesServer } from "../ReleaseNotesServer";

/**
 * Decodes HTML entities in the release notes.
 */
const decodeHTML = (html: string) => {
    const txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
};

const ReleaseNotesModal: React.FC<{ open: boolean; handleClose: () => void }> = ({ open, handleClose }) => {
    const [releases, setReleases] = useState<{ date: string; name: string; notes: string }[]>([]);

    useEffect(() => {
        if (open) {
            ReleaseNotesServer().then(setReleases);
        }
    }, [open]);

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            aria-labelledby="release-notes-title"
            maxWidth={false}
            fullWidth
            PaperProps={{
                style: { height: "90vh", width: "90vw" },
            }}
        >
            <DialogTitle id="release-notes-title">{"Zork AI Release Notes"}</DialogTitle>
            <DialogContent dividers>
                {releases.length === 0 ? (
                    <p>Loading release notes...</p>
                ) : (
                    <div style={{ fontSize: "1rem", lineHeight: "1.5" }}>
                        {releases.map((release) => (
                            <div key={release.date} style={{marginBottom: "20px"}}>
                                <h3 style={{fontSize: "1.2rem", fontWeight: "bold", margin: "10px 0"}}>
                                    {release.name} - {new Date(release.date).toLocaleDateString()}
                                </h3>
                                <div
                                    dangerouslySetInnerHTML={{__html: decodeHTML(release.notes)}}
                                    style={{
                                        whiteSpace: "normal",
                                        wordWrap: "break-word"
                                    }}
                                />
                                <hr style={{border: "1px solid #ccc", margin: "20px 0"}}/>
                            </div>
                        ))}
                    </div>
                )}
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} autoFocus>
                    Close
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ReleaseNotesModal;
