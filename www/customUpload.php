
<?php

if(!isset($_POST["secret"]))
{
    echo "Error, no secret provided.";
    exit;
}

if ( $_POST["secret"] !== "87ajdf898##@@jjKJA" )
{
    echo "Error secret does not match.";
    exit;
}

// For backwards compatability
if(!isset($_FILES) && isset($HTTP_POST_FILES))
{
    $_FILES = $HTTP_POST_FILES;
}

if ($_FILES['file']['error'] !== UPLOAD_ERR_OK)
{
    echo "Upload error set " . $_FILES['file']['error'];
    exit;
}

/*
if ($_FILES['file']['type'] !== "application/zip")
{
    echo "Mime type mismatch";
    exit;
}*/

// Check if file already exists
$uploadfile =  'uploadedFiles/' . $_FILES['file']['name'];
if (file_exists($uploadfile)) 
{
    echo "File already exists.";
    exit;
}

move_uploaded_file($_FILES['file']['tmp_name'], $uploadfile); 

echo "Upload of " . $uploadfile . " succesful.";

?>