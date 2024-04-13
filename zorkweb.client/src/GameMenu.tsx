import React from 'react';
import { Button } from '@mui/material';
import AboutMenu from './AboutMenu'; // make sure to import from correct path
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import CloudDownloadIcon from '@mui/icons-material/CloudDownload';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';

const GameMenu: React.FC = () => (
    <div className="p-1 grid grid-cols-10 bg-gray-200 gap-2">
        <div className="col-span-4">
            <AboutMenu/>
        </div>
        <Button className="col-span-2" size="small" variant="contained" startIcon={<CloudUploadIcon />} >Save</Button>
        <Button className="col-span-2" size="small" variant="contained" startIcon={<CloudDownloadIcon />}>Restore</Button>
        <Button className="col-span-2" size="small" variant="contained" startIcon={<RestartAltIcon />}>Restart</Button>
    </div>
);

export default GameMenu;