<?php
if(isset($_GET["name"])){

    // Process download
    $filepath = "uploadedFiles/" . $_GET["name"];
   // echo $filepath;
    if(file_exists($filepath)) 
    {
        header('Content-Description: File Transfer');
        header('Content-Type: application/octet-stream');
        header('Content-Disposition: attachment; filename="'.basename($filepath).'"');
        header('Expires: 0');
        header('Cache-Control: must-revalidate');
        header('Pragma: public');
        header('Content-Length: ' . filesize($filepath));
        flush(); // Flush system output buffer
        readfile($filepath);
        exit;
    }
    else{
      echo "File does not exist.";
    }
}
?>