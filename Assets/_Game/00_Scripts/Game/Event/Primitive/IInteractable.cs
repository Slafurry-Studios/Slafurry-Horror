public interface IInteractable
{
    string GetPromptText();
    bool CanInteract();
    void Interact();
}