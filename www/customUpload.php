<!DOCTYPE html>
<html lang="en">
<head>
    <title>PHP Upload file</title>
</head>
<body>
<?php
if(isset($_POST["name"]) && isset($_POST["objData"]))
{ 
    $name = $_POST["name"];
    $handle = fopen("uploadedFiles/" . $name, "w");
    if ( $handle )
    {
        // Write data to the file
        $data = $_POST["objData"];
        file_put_contents($handle, $data) or die("ERROR: Cannot write the file.");
        fflush($handle);
        fclose($handle);
    }
   // readfile("tile.obj");
//    exit;
}
?>
</body>